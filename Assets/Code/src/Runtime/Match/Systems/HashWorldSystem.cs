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

namespace HouraiTeahouse.FantasyCrescendo.Matches {

/// <summary>
/// A system for deterministic producing hashes based on select components
/// of interest.
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public class HashWorldSystem : SystemBase {

  public struct HashResult : IComparable<HashResult> {
    public static readonly int Size = UnsafeUtility.SizeOf<HashResult>();

    public int ID;
    public Hash Hash;

    public int CompareTo(HashResult other) => ID.CompareTo(other.ID);
  }

  const int kMaxIndex = 6;
  NativeArray<Hash> _result;

  protected override void OnCreate() {
    _result = new NativeArray<Hash>(kMaxIndex, Allocator.Persistent);
  }

  protected unsafe override void OnUpdate() {
    int entityCount = EntityManager.Debug.EntityCount;
    var results = new NativeMultiHashMap<int, HashResult>(entityCount * kMaxIndex, Allocator.TempJob);
    var writer = results.AsParallelWriter();
    var jobs = new NativeArray<JobHandle>(kMaxIndex, Allocator.Temp);
    var idx = 0;
    jobs[idx] = HashComponents<PlayerComponent>(idx++, writer);
    jobs[idx] = HashComponents<Translation>(idx++, writer);
    jobs[idx] = HashComponents<Rotation>(idx++, writer);
    jobs[idx] = HashComponents<Scale>(idx++, writer);
    jobs[idx] = HashComponents<Hitbox>(idx++, writer);
    jobs[idx] = HashComponents<Hurtbox>(idx++, writer);
    Dependency = JobHandle.CombineDependencies(jobs);

    NativeArray<Hash> finalResult = _result;
    for (var i = 0; i < kMaxIndex; i++) {
      jobs[i] = Job
      .WithName("CollectHashes")
      .WithNativeDisableContainerSafetyRestriction(results)
      .WithNativeDisableContainerSafetyRestriction(finalResult)
      .WithCode(() => {
        NativeArray<HashResult>? slice = results.CopyValuesForKey(i);
        if (slice == null) {
          finalResult[i] = 0;
          return;
        }

        var hashResults = slice.Value;
        var hashes = new NativeArray<Hash>(hashResults.Length, Allocator.Temp);
        hashResults.Sort();
        for (var j = 0; j < hashResults.Length; j++) {
          hashes[j] = hashResults[j].Hash;
        }
        finalResult[i] = XXHash.Hash64((byte*)hashes.GetUnsafeReadOnlyPtr(), 
                                              entityCount * sizeof(Hash));
      }).Schedule(Dependency);
    }
    Dependency = JobHandle.CombineDependencies(jobs);
    results.Dispose(Dependency);
  }

  protected override void OnDestroy() {
    _result.Dispose();
  }

  public unsafe Hash GetWorldHash() {
    CompleteDependency();
    return XXHash.Hash64((byte*)_result.GetUnsafeReadOnlyPtr(),
                         kMaxIndex * sizeof(Hash));
  }

  unsafe JobHandle HashComponents<T>(int index,  NativeMultiHashMap<int, HashResult>.ParallelWriter writer) 
                                     where T : struct, IComponentData {
    var query = GetEntityQuery(ComponentType.ReadOnly<T>());
    return new HashComponentsJob<T> {
      Index = index,
      EntityHandle = EntityManager.GetArchetypeChunkEntityType(),
      ComponentHandle = GetArchetypeChunkComponentType<T>(true),
      Results = writer
    }.ScheduleParallel(query, Dependency);
  }

  [BurstCompile]
  struct HashComponentsJob<T> : IJobChunk where T : struct, IComponentData {
    public int Index;
    [ReadOnly] public ArchetypeChunkEntityType EntityHandle;
    [ReadOnly] public ArchetypeChunkComponentType<T> ComponentHandle;
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

}

}