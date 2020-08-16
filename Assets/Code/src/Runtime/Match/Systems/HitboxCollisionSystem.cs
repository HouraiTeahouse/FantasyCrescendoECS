using HouraiTeahouse.FantasyCrescendo.Utils;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Physics;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct HitboxCollision : IComparable<HitboxCollision> {
  public Entity OriginPlayer;
  public Entity TargetPlayer;

  public Hitbox Hitbox;
  public Hurtbox Hurtbox;

  public int CompareTo(HitboxCollision other) {
    // Higher values (SHIELD, INVINCIBLE, etc) should come first.
    int result = -((byte)Hurtbox.Type).CompareTo((byte)other.Hurtbox.Type);
    if (result  != 0) return result;
    if (((Hitbox.Flags ^ other.Hitbox.Flags) & HitboxFlags.TRASCENDENT_PRIORITY) != 0) {
      return Hitbox.Is(HitboxFlags.TRASCENDENT_PRIORITY) ? -1 : 1;
    }
    return -Hitbox.Priority.CompareTo(other.Hitbox.Priority);
  }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public class HitboxCollisionSystem : SystemBase {

  BuildPhysicsWorld _worldBuildSystem;
  EndFramePhysicsSystem _endFramePhysicsSystem;
  NativeMultiHashMap<Entity, HitboxCollision> _collisions;

  protected override void OnCreate() {
    _worldBuildSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    _collisions = new NativeMultiHashMap<Entity, HitboxCollision>(
      /*capacity=*/MatchConfig.kMaxSupportedPlayers * CharacterFrame.kMaxPlayerHitboxCount,
      Allocator.Persistent);
  }

  protected unsafe override void OnUpdate() {
    PhysicsWorld physicsWorld = _worldBuildSystem.PhysicsWorld;
    NativeMultiHashMap<Entity, HitboxCollision> collisions = _collisions;
    collisions.Clear();
    var collisionWriter = collisions.AsParallelWriter();
    var hurtboxes = GetComponentDataFromEntity<Hurtbox>(true);

    Entities
    .WithName("QueryHitboxCollisons")
    .WithReadOnly(physicsWorld)
    .WithReadOnly(hurtboxes)
    .ForEach((Entity entity, ref Hitbox hitbox, ref HitboxState state, in LocalToWorld transform) => {
      if (!state.Enabled) return;

      float3 position = TransformPoint(transform.Value, float3.zero);
      var hits = new NativeList<ColliderCastHit>(Allocator.Temp);
      physicsWorld.CastCollider(new ColliderCastInput {
        Collider = (Collider*)SphereCollider.Create(new SphereGeometry {
          Center = float3.zero,
          Radius = hitbox.Radius
        }, new CollisionFilter {
          BelongsTo = (uint)PhysicsLayers.HITBOX,
          CollidesWith = (uint)PhysicsLayers.HITBOX,
        }).GetUnsafePtr(),
        Start = state.PreviousPosition ?? position,
        End = position,
        Orientation = float4.zero
      }, ref hits);
      state.PreviousPosition = position;

      for (var i = 0; i < hits.Length; i++) {
        var hit = hits[i];
        if (!hurtboxes.HasComponent(hit.Entity)) continue;
        Hurtbox hurtbox = hurtboxes[hit.Entity];
        if (!hurtbox.Enabled || hurtbox.Player == Entity.Null || hurtbox.Player == state.Player) continue;

        collisionWriter.Add(hit.Entity, new HitboxCollision {
          OriginPlayer = entity,
          TargetPlayer = hurtbox.Player,

          Hitbox = hitbox,
          Hurtbox = hurtbox,
        });
      }
    }).ScheduleParallel();

    Entities
    .WithName("ApplyCollisions")
    .WithReadOnly(collisions)
    .ForEach((Entity entity, ref PlayerComponent player, in PlayerConfig config, in CharacterFrame frame) => {
      NativeArray<HitboxCollision>? playerCollisions = CopyValuesForKey(collisions, entity);
      if (playerCollisions == null) return;

      // Sort collisions
      playerCollisions.Value.Sort();

    }).Schedule();
  }

  protected override void OnDestroy() {
    _collisions.Dispose();
  }

  static float3 TransformPoint(in float4x4 mat, in float3 p) {
    float4 point = math.mul(mat, new float4(p, 1f));
    return point.xyz * (1f / point.w);
  }

  static NativeArray<TValue>? CopyValuesForKey<TKey, TValue>(NativeMultiHashMap<TKey, TValue> map, TKey key) 
                                                             where TKey : struct, IEquatable<TKey>
                                                             where TValue : struct {
    if (!map.ContainsKey(key)) return null;

    var count = map.CountValuesForKey(key);
    var iterator = map.GetValuesForKey(key);
    var values = new NativeArray<TValue>(count, Allocator.Temp);
    for (var i = 0; i < count && iterator.MoveNext(); i++) {
      values[i] = iterator.Current;
    }

    return values;
  }

}

}