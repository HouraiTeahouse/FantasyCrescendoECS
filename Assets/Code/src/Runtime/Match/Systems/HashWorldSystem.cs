using System;
using Unity.Assertions;
using Unity.Core;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using Unity.Entities;

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
    public ulong Hash;

    public int CompareTo(HashResult other) => ID.CompareTo(other.ID);
  }

  const int kMaxIndex = 6;
  NativeArray<HashResult> _result;

  protected override void OnCreate() {
    _result = new NativeArray<HashResult>(kMaxIndex, Allocator.Persistent);
  }

  protected unsafe override void OnUpdate() {
    var allJobs = Dependency;
    var entities = EntityManager.UniversalQuery.ToEntityArray(Allocator.TempJob);
    var entityCount = entities.Length;
    NativeArray<HashResult> finalResult = _result;
    var results = new NativeArray<HashResult>(entities.Length * kMaxIndex, Allocator.TempJob);
    var jobs = new NativeArray<JobHandle>(kMaxIndex, Allocator.Temp);
    var idx = 0;
    jobs[idx] = HashComponents<PlayerComponent>(idx++, entities, results);
    jobs[idx] = HashComponents<Translation>(idx++, entities, results);
    jobs[idx] = HashComponents<Rotation>(idx++, entities, results);
    jobs[idx] = HashComponents<Scale>(idx++, entities, results);
    jobs[idx] = HashComponents<Hitbox>(idx++, entities, results);
    jobs[idx] = HashComponents<Hurtbox>(idx++, entities, results);
    Dependency = JobHandle.CombineDependencies(jobs);
    entities.Dispose(Dependency);

    for (var i = 0; i < kMaxIndex; i++) {
      jobs[i] = Job
      .WithName("CollectHashes")
      .WithNativeDisableContainerSafetyRestriction(results)
      .WithNativeDisableContainerSafetyRestriction(finalResult)
      .WithCode(() => {
        var slice = new NativeSlice<HashResult>(results, i * entityCount, entityCount);
        slice.Sort();
        finalResult[i] = new HashResult {
          ID = i,
          Hash = XXHash.Hash64((byte*)slice.GetUnsafeReadOnlyPtr(), 
                               entityCount * HashResult.Size)
        };
      }).Schedule(Dependency);
    }
    Dependency = JobHandle.CombineDependencies(jobs);
    results.Dispose(Dependency);
  }

  protected override void OnDestroy() {
    _result.Dispose();
  }

  public unsafe ulong GetWorldHash() {
    CompleteDependency();
    return XXHash.Hash64((byte*)_result.GetUnsafeReadOnlyPtr(),
                         kMaxIndex * HashResult.Size);
  }

  unsafe JobHandle HashComponents<T>(int index, NativeArray<Entity> entities, 
                                     NativeArray<HashResult> results) 
                                     where T : struct, IComponentData {
    return new HashComponentsJob<T> {
      Index = index,
      Entities = entities,
      Components = GetComponentDataFromEntity<T>(true),
      Results = results
    }.Schedule(entities.Length, 64, Dependency);
  }

  [BurstCompile]
  struct HashComponentsJob<T> : IJobParallelFor where T : struct, IComponentData {
    public int Index;
    [ReadOnly] public NativeArray<Entity> Entities;
    [ReadOnly] public ComponentDataFromEntity<T> Components;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<HashResult> Results;

    public unsafe void Execute(int idx) {
      var entity = Entities[idx];
      ulong hash = 0;
      if (Components.HasComponent(entity)) {
        T component = Components[entity];
        hash = XXHash.Hash64((byte*)UnsafeUtility.AddressOf(ref component), 
                              UnsafeUtility.SizeOf<T>());
      }
      var resultIdx = Index * Entities.Length + idx;
      Results[resultIdx] = new HashResult { ID = entity.Index, Hash = hash };
    }

  }

}

}