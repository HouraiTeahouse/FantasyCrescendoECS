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

  WorldStorageID _worldId;
  readonly List<World> _worlds;
  readonly Queue<WorldStorageID> _pool;
  readonly HashSet<WorldStorageID> _inactive;

  public int Count => _worlds.Count - _pool.Count;
  public int TotalCount => _worlds.Count;

  public WorldStorage() {
    _worlds = new List<World>();
    _pool = new Queue<WorldStorageID>();
    _inactive = new HashSet<WorldStorageID>();
  }

  public WorldStorageID Get(out World world) {
    if (_pool.Count > 0) {
      WorldStorageID idx = _pool.Dequeue();
      world = _worlds[(int)idx];
      _inactive.Remove(idx);
      return idx;
    } else {
      world = new World($"Pool_World_{_worldId++}");
      _worlds.Add(world);
      return (uint)(_worlds.Count - 1);
    }
  }

  public bool TryGetValue(WorldStorageID id, out World world) {
    bool valid = id >= 0 && id < _worlds.Count && !_inactive.Contains(id);
    world = valid ? _worlds[(int)id] : null;
    return valid;
  }

  public void Remove(WorldStorageID id) {
    if (_inactive.Contains(id)) return;
    _pool.Enqueue(id);
    _inactive.Add(id);
    World world = _worlds[(int)id];
    world.EntityManager.DestroyEntity(world.EntityManager.UniversalQuery);
  }

  public void Dispose() {
    foreach (var world in _worlds) {
      try {
        world.Dispose();
      } catch (ArgumentException exec)  {
      }
    }
    _worlds.Clear();
    _pool.Clear();
    _inactive.Clear();
  }

}

public unsafe abstract class RollbackMatch : Match {

  protected BackrollSessionConfig BackrollConfig { get; }
  protected BackrollSession<PlayerInput> Session { get; private set; }
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
    Session.AdvanceFrame();
    // Timeout is in milliseconds
    Session.Idle((int)(Time.fixedDeltaTime * 20000));
  }
  
  protected unsafe override void SampleInputs() {
    // Should be 40 bytes
    var length = UnsafeUtility.SizeOf<PlayerInput>() * MatchConfig.kMaxSupportedPlayers;
    byte* inputs = stackalloc byte[length];
    Session.SyncInput(inputs, length);
    InjectInputs(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<PlayerInput>(inputs, length, Allocator.Temp));
  }

  protected unsafe void Serialize(ref Sync.SavedFrame frame) {
    WorldStorageID id = WorldStorage.Get(out World saveState);

    EntityManager.BeginExclusiveEntityTransaction();
    saveState.EntityManager.CopyAndReplaceEntitiesFrom(EntityManager);
    EntityManager.EndExclusiveEntityTransaction();

    frame = new Sync.SavedFrame {
      // NOTE: THIS IS A HUGE HACK. If this ever gets dereferenced, the
      // game will hard crash via segmentation fault.
      Buffer = (byte*)id,
      // TODO(james7132): Hash the state consistently.
      Checksum = 0,
      Size = 0,
    };
  }

  protected unsafe void Deserialize(void* buffer, int len) {
    // NOTE: THIS IS A HUGE HACK. If this ever gets dereferenced, the
    // game will hard crash via segmentation fault.
    Assert.IsTrue(WorldStorage.TryGetValue((WorldStorageID)buffer, out World world));
    Assert.IsTrue(len == 0);
    EntityManager.CopyAndReplaceEntitiesFrom(World.EntityManager);
  }

  protected unsafe void ReleaseWorld(IntPtr buffer) {
    // NOTE: THIS IS A HUGE HACK. If this ever gets dereferenced, the
    // game will hard crash via segmentation fault.
    WorldStorage.Remove((WorldStorageID)buffer);
  }

  protected virtual BackrollSession<PlayerInput> CreateBackrollSession() {
    return HouraiTeahouse.Backroll.Backroll.StartSession<PlayerInput>(BackrollConfig);
  }

}

}
