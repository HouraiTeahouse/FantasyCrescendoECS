using HouraiTeahouse.Backroll;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Assertions;
using Unity.Entities;
using Time = UnityEngine.Time;

using WorldPoolID = System.UInt32;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

/// <summary>
/// A managed pool of ECS Worlds. Useful for saving multiple snapshots
/// of a world.
/// </summary>
public sealed class WorldPool : IDisposable {

  /// <summary>
  /// A singleton instance.
  /// </summary>
  public static readonly WorldPool Instance = new WorldPool();

  WorldPoolID _worldId;
  readonly List<World> _worlds;
  readonly Queue<WorldPoolID> _pool;
  readonly HashSet<WorldPoolID> _inactive;

  /// <summary>
  /// The total number of active worlds.
  /// </summary>
  public int Count => _worlds.Count - _pool.Count;

  /// <summary>
  /// The total number of worlds, including inactive ones.
  /// </summary>
  public int TotalCount => _worlds.Count;

  public WorldPool() {
    _worlds = new List<World>();
    _pool = new Queue<WorldPoolID>();
    _inactive = new HashSet<WorldPoolID>();
  }

  /// <summary>
  /// Gets a free world from the storage. If a free World does not exist in 
  /// the internal pool, a new one will be created.
  /// </summary>
  /// <param name="world">an out parameter for the fetched World.</param>
  /// <returns>the ID for the World</returns>
  public WorldPoolID Get(out World world) {
    if (_pool.Count > 0) {
      WorldPoolID idx = _pool.Dequeue();
      world = _worlds[(int)idx];
      _inactive.Remove(idx);
      return idx;
    } else {
      world = new World($"Pool_World_{_worldId++}");
      _worlds.Add(world);
      return (uint)(_worlds.Count - 1);
    }
  }

  /// <summary>
  /// Tries to get an active World from the storage.
  /// </summary>
  /// <param name="id">the ID of the world to fetch</param>
  /// <param name="world">an out parameter for the fetched World. Will be null if there wasn't one found.</param>
  /// <returns>true if an world was found, false if no World exists or was inactive.</returns>
  public bool TryGetValue(WorldPoolID id, out World world) {
    bool valid = IsActive(id);
    world = valid ? _worlds[(int)id] : null;
    return valid;
  }

  /// <summary>
  /// Gets if 
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  public bool IsActive(WorldPoolID id) {
    return id >= 0 && id < _worlds.Count && !_inactive.Contains(id);
  }

  /// <summary>
  /// Removes a world from the active set of Worlds based on it's ID.
  /// It will return the world to the inactive pool and delete all entities
  /// from the world.
  /// A noop if the ID does not correspond to an active world.
  /// </summary>
  /// <param name="id">the ID of the world to </param>
  public void Remove(WorldPoolID id) {
    if (!IsActive(id)) return;
    _pool.Enqueue(id);
    _inactive.Add(id);
    EntityManager manager = _worlds[(int)id].EntityManager;
    manager.DestroyEntity(manager.UniversalQuery);
  }

  /// <summary>
  /// Deactivates all active worlds and returns them to the pool.
  /// </summary>
  public void Clear() {
    for (uint i = 0; i < _worlds.Count; i++) {
      Remove(i);
    }
  }

  /// <summary>
  /// Dispose of the storage and all of the Worlds managed by the storage.
  /// </summary>
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

public unsafe abstract class RollbackMatch : RecordableMatch {

  protected BackrollSessionConfig BackrollConfig { get; }
  protected BackrollSession<PlayerInput> Session { get; private set; }
  protected WorldPool SavedStates { get; }

  readonly HashWorldSystem _hasher;

  public RollbackMatch(MatchConfig config, BackrollSessionConfig backrollConfig,
                       World world = null) : base(config, world) {
    _hasher = World.GetOrCreateSystem<HashWorldSystem>();
    SavedStates = WorldPool.Instance;
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

  public override void Dispose() {
    base.Dispose();
    SavedStates.Clear();
  }
  
  protected unsafe override void SampleInputs() {
    // Should be 40 bytes
    var length = UnsafeUtility.SizeOf<PlayerInput>() * MatchConfig.kMaxSupportedPlayers;
    byte* inputs = stackalloc byte[length];
    Session.SyncInput(inputs, length);
    InjectInputs(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<PlayerInput>(inputs, length, Allocator.Temp));
  }

  protected unsafe void Serialize(ref Sync.SavedFrame frame) {
    WorldPoolID id = SavedStates.Get(out World saveState);

    EntityManager.BeginExclusiveEntityTransaction();
    saveState.EntityManager.CopyAndReplaceEntitiesFrom(EntityManager);
    EntityManager.EndExclusiveEntityTransaction();

    // Get the hash and reduce
    ulong hash = _hasher.GetWorldHash();
    unchecked {
      frame = new Sync.SavedFrame {
        // NOTE: THIS IS A HUGE HACK. If this ever gets dereferenced, the
        // game will hard crash via segmentation fault.
        Buffer = (byte*)id,
        // Reduce the 64 bit hash by XORing the top 32 bits with the bottom 32
        Checksum = (int)((hash & 0xffffffff) ^ ((hash >> 32) & 0xffffffff)),
        Size = 0,
      };
    }
  }

  protected unsafe void Deserialize(void* buffer, int len) {
    // NOTE: THIS IS A HUGE HACK. If this ever gets dereferenced, the
    // game will hard crash via segmentation fault.
    Assert.IsTrue(SavedStates.TryGetValue((WorldPoolID)buffer, out World world));
    Assert.IsTrue(len == 0);
    EntityManager.CopyAndReplaceEntitiesFrom(World.EntityManager);
  }

  protected unsafe void ReleaseWorld(IntPtr buffer) {
    // NOTE: THIS IS A HUGE HACK. If this ever gets dereferenced, the
    // game will hard crash via segmentation fault.
    var id = (WorldPoolID)buffer;
    Assert.IsTrue(SavedStates.IsActive(id));
    SavedStates.Remove(id);
    Assert.IsTrue(!SavedStates.IsActive(id));
  }

  protected virtual BackrollSession<PlayerInput> CreateBackrollSession() {
    return HouraiTeahouse.Backroll.Backroll.StartSession<PlayerInput>(BackrollConfig);
  }

}

}
