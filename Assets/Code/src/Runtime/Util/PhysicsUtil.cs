using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public static class PhysicsUtil {

  public static unsafe void SphereCast(in PhysicsWorld world, float3 from, float3 to, float radius, 
                                       ref NativeList<ColliderCastHit> hits) {
    BlobAssetReference<Collider> collider = SphereCollider.Create(new SphereGeometry {
      Center = float3.zero,
      Radius = radius
    });

    world.CastCollider(new ColliderCastInput {
      Collider = (Collider*)collider.GetUnsafePtr(),
      Start = from,
      End = to,
      Orientation = float4.zero
    }, ref hits);

    collider.Dispose();
  }

}

}