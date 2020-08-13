using HouraiTeahouse.FantasyCrescendo.Utils;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Physics;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public class HitboxCollisionSystem : SystemBase {

  BuildPhysicsWorld _worldBuildSystem;
  EndFramePhysicsSystem _endFramePhysicsSystem;

  protected override void OnCreate() {
    _worldBuildSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
  }

  protected override void OnUpdate() {
    PhysicsWorld physicsWorld = _worldBuildSystem.PhysicsWorld;

    Entities
    .WithName("QueryHitboxCollisons")
    .WithReadOnly(physicsWorld)
    .ForEach((ref Hitbox hitbox, in HitboxState state, in LocalToWorld transform) => {
      float3 position = TransformPoint(transform.Value, float3.zero);
      var hits = new NativeList<ColliderCastHit>(Allocator.Temp);
      PhysicsUtil.SphereCast(physicsWorld, state.PreviousPosition ?? default(float3), position, 
                             hitbox.Radius, ref hits);
    }).ScheduleParallel();

  }

  static float3 TransformPoint(in float4x4 mat, in float3 p) {
    float4 point = math.mul(mat, new float4(p, 1f));
    return point.xyz * (1f / point.w);
  }

}

}