using HouraiTeahouse.Backroll;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Assertions;
using Unity.Entities;
using Time = UnityEngine.Time;

using WorldStorageID = System.UInt32;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public sealed class WorldStorage : IDisposable {

  readonly Dictionary<WorldStorageID, World> _worlds;
  WorldStorageID _nextId;

  public WorldStorageID NextID => _nextId;
  public int Count => _worlds.Count;

  public WorldStorage() {
    _nextId = 0;
    _worlds = new Dictionary<WorldStorageID, World>();
  }

  public WorldStorageID Add(World world) {
    Assert.IsNotNull(world);
    _worlds[_nextId] = world;
    return _nextId++;
  }

  public bool TryGetValue(WorldStorageID id, out World world) =>
    _worlds.TryGetValue(_nextId, out world);

  public void Remove(WorldStorageID id) {
    if (_worlds.TryGetValue(id, out World world))  {
      _worlds.Remove(id);
      world.Dispose();
    }
  }

  public void Dispose() {
    foreach (var world in _worlds.Values) {
      world.Dispose();
    }
  }

}

public unsafe abstract class RollbackMatch : Match {

  protected BackrollSessionConfig BackrollConfig { get; }
  protected BackrollSession<PlayerInput> BackrollSession { get; private set; }
  protected WorldStorage WorldStorage { get; }

  public RollbackMatch(MatchConfig config, BackrollSessionConfig backrollConfig,
                       World world = null) : base(config, world) {
    WorldStorage = new WorldStorage();
    backrollConfig.Callbacks = new BackrollSessionCallbacks {
      SaveGameState = Serialize,
      LoadGameState = Deserialize,
      FreeBuffer = ReleaseWorld,
      AdvanceFrame = Step,
    };
    BackrollConfig = backrollConfig;
    Assert.IsTrue(BackrollConfig.IsValid);
  }

  public override void Update() {
    // TODO(james7132): Sample local inputs
    BackrollSession.AdvanceFrame();
    // Timeout is in milliseconds
    BackrollSession.Idle((int)(Time.fixedDeltaTime * 20000));
  }
  
  protected unsafe override void SampleInputs() {
    // Should be 40 bytes
    var length = UnsafeUtility.SizeOf<PlayerInput>() * MatchConfig.kMaxSupportedPlayers;
    byte* inputs = stackalloc byte[length];
    BackrollSession.SyncInput(inputs, length);
    InjectInputs(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<PlayerInput>(inputs, length, Allocator.Temp));
  }

  protected unsafe virtual void Serialize(ref Sync.SavedFrame frame) {
    EntityManager.BeginExclusiveEntityTransaction();
    var saveState = new World("Saved_Frame_" + WorldStorage.NextID);
    saveState.EntityManager.CopyAndReplaceEntitiesFrom(EntityManager);
    EntityManager.EndExclusiveEntityTransaction();

    frame.Size = UnsafeUtility.SizeOf<WorldStorageID>();
    frame.Buffer = (byte*)UnsafeUtility.Malloc(frame.Size, UnsafeUtility.AlignOf<WorldStorageID>(),
                                               Allocator.Persistent);
    *((WorldStorageID*)frame.Buffer) = WorldStorage.Add(saveState);
  }

  protected unsafe virtual void Deserialize(void* buffer, int len) {
    Assert.AreEqual(sizeof(WorldStorageID), len);
    Assert.IsTrue(WorldStorage.TryGetValue(*(WorldStorageID*)buffer, out World world));
    EntityManager.CopyAndReplaceEntitiesFrom(world.EntityManager);
  }

  protected unsafe virtual void ReleaseWorld(IntPtr buffer) {
    WorldStorage.Remove(*(WorldStorageID*)buffer);
    UnsafeUtility.Free((void*)buffer, Allocator.Persistent);
  }

}

}
