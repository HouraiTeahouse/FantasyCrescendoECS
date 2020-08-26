using HouraiTeahouse.FantasyCrescendo.Authoring;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct CameraTarget : IComponentData {}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class MatchCameraSystem : SystemBase {

  NativeList<float3> _targetPositions;

  protected override void OnCreate() {
    _targetPositions = new NativeList<float3>(Allocator.Persistent);
  }

  protected override void OnUpdate() {
    NativeList<float3> positions  =_targetPositions;
    positions.Clear();

    Entities
    .WithName("CollectTargets")
    .ForEach((in CameraTarget target, in LocalToWorld transform) => {
      positions.Add(math.transform(transform.Value, float3.zero));
    }).Schedule();

    Entities
    .WithName("UpdateCamera")
    .WithReadOnly(positions)
    .WithoutBurst()
    .ForEach((CameraController controller) => {
      if (!GetTargetBoudns(positions, out Bounds bounds)) return;
      controller.Move(bounds);
    }).Run();
  }

  protected override void OnDestroy() {
    _targetPositions.Dispose();
  }

  static bool GetTargetBoudns(NativeList<float3> positions, out Bounds bounds) {
    bool found = false;
    bounds = new Bounds();
    for (var i = 0; i < positions.Length; i++) {
      var pos = positions[i];
      if (!found) {
        bounds = new Bounds(pos, Vector3.zero);
      } else {
        bounds.Encapsulate(pos);
      }
      found = true;
    }
    return found;
  }

}

}
