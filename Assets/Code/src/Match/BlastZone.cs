using HouraiTeahouse.FantasyCrescendo.Utils;
using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class BlastZone : MonoBehaviour {

#pragma warning disable 0649
  [SerializeField] Bounds bounds;
#pragma warning restore 0649

  BlastZoneSystem System => World.DefaultGameObjectInjectionWorld?.GetOrCreateSystem<BlastZoneSystem>();

  void Start() {
    System.AddBoundingBox((Bounds2D)GetWorldBounds(bounds));
  }

  void OnDestroy() {
    System?.Reset();
  }

#if UNITY_EDITOR
  void OnDrawGizmos() {
    var worldBounds = (Bounds)(Bounds2D)GetWorldBounds(bounds);
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireCube(worldBounds.center, worldBounds.extents);
  }
#endif

  Bounds GetWorldBounds(Bounds bounds) {
    return new Bounds {
      min = transform.TransformPoint(bounds.min),
      max = transform.TransformPoint(bounds.max),
    };
  }

}

}

