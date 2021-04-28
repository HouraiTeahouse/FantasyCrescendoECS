using HouraiTeahouse.FantasyCrescendo;
using HouraiTeahouse.FantasyCrescendo.Matches;
using HouraiTeahouse.FantasyCrescendo.Utils;
using System;
using Unity.Assertions;
using Unity.Core;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using Unity.Entities;

using Hash = System.UInt64;

[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<PlayerComponent>))]
[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<Translation>))]
[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<Rotation>))]
[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<Scale>))]
[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<HitboxState>))]
[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<Hitbox>))]
[assembly: RegisterGenericJobType(typeof(HashWorldSystem.HashComponentsJob<Hurtbox>))]

namespace HouraiTeahouse.FantasyCrescendo.Matches {

/// <summary>
/// A system for deterministic producing hashes based on select components
/// of interest.
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public class HashWorldSystem : SystemBase {

  struct HashResult : IComparable<HashResult> {
    public static readonly int Size = UnsafeUtility.SizeOf<HashResult>();

    public int ID;
    public Hash Hash;

    public int CompareTo(HashResult other) => ID.CompareTo(other.ID);
  }

  const int kMaxIndex = 7;
  NativeArray<Hash> _result;

  protected override void OnCreate() {
    _result = new NativeArray<Hash>(kMaxIndex, Allocator.Persistent);
  }

  protected override void OnUpdate() {
    int entityCount = EntityManager.Debug.EntityCount;
    var results = new NativeMultiHashMap<int, HashResult>(entityCount * kMaxIndex, Allocator.TempJob);
    var writer = results.AsParallelWriter();

    var jobs = new NativeArray<JobHandle>(kMaxIndex, Allocator.Temp);
    var idx = 0;
    jobs[idx] = HashComponents<PlayerComponent>(idx++, writer);
    jobs[idx] = HashComponents<Translation>(idx++, writer);
    jobs[idx] = HashComponents<Rotation>(idx++, writer);
    jobs[idx] = HashComponents<Scale>(idx++, writer);
    jobs[idx] = HashComponents<HitboxState>(idx++, writer);
    jobs[idx] = HashComponents<Hitbox>(idx++, writer);
    jobs[idx] = HashComponents<Hurtbox>(idx++, writer);
    Dependency = JobHandle.CombineDependencies(jobs);

    Dependency = new CollectHashes {
      Results = results,
      Output = _result
    }.Schedule(_result.Length, 1, Dependency);
    results.Dispose(Dependency);
  }

  protected override void OnDestroy() {
    _result.Dispose();
  }

  public unsafe Hash GetWorldHash() {
    CompleteDependency();
    return _result.XXHash64();
  }

  public Hash[] GetComponentHashes() {
    CompleteDependency();
    var hashes = new Hash[kMaxIndex];
    _result.CopyTo(hashes);
    return hashes;
  }

  unsafe JobHandle HashComponents<T>(int index,  NativeMultiHashMap<int, HashResult>.ParallelWriter writer) 
                                     where T : struct, IComponentData {
    Assert.IsTrue(index >= 0 && index < kMaxIndex);
    var query = GetEntityQuery(ComponentType.ReadOnly<T>());
    return new HashComponentsJob<T> {
      Index = index,
      EntityHandle = EntityManager.GetEntityTypeHandle(),
      ComponentHandle = GetComponentTypeHandle<T>(true),
      Results = writer
    }.ScheduleParallel(query, Dependency);
  }

  [BurstCompile]
  public struct HashComponentsJob<T> : IJobChunk where T : struct, IComponentData {
    public int Index;
    [ReadOnly] public EntityTypeHandle EntityHandle;
    [ReadOnly] public ComponentTypeHandle<T> ComponentHandle;
    [NativeDisableContainerSafetyRestriction]
    public NativeMultiHashMap<int, HashResult>.ParallelWriter Results;

    public unsafe void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
      NativeArray<Entity> entities = chunk.GetNativeArray(EntityHandle);
      NativeArray<T> components = chunk.GetNativeArray(ComponentHandle);
      int size = UnsafeUtility.SizeOf<T>();
      byte* startPtr = (byte*)components.GetUnsafeReadOnlyPtr();
      for (var i = 0; i < components.Length; i++) {
        Results.Add(Index, new HashResult { 
          ID = entities[i].Index, 
          Hash = XXHash.Hash64(startPtr + i * size, size)
        });
      }
    }
  }

  [BurstCompile]
  struct CollectHashes : IJobParallelFor {
    [ReadOnly] public NativeMultiHashMap<int, HashResult> Results;
    public NativeArray<Hash> Output;

    public void Execute(int idx) {
        NativeArray<HashResult>? slice = Results.CopyValuesForKey(idx);
        if (slice == null) {
          Output[idx] = 0;
          return;
        }
        var results = slice.Value;
        results.Sort();
        var hashes = new NativeArray<Hash>(results.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        for (var j = 0; j < hashes.Length; j++) {
          hashes[j] = results[j].Hash;
        }
        Output[idx] = hashes.XXHash64();
    }
  }

}

}