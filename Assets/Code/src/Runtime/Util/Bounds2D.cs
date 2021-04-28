using UnityEngine;
using Unity.Mathematics;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public struct Bounds2D {
  public float2 min;
  public float2 max;

  public bool Contains(float2 point) {
    var res = (point >= min) & (point <= max);
    return res.x && res.y;
  }

  public static explicit operator Bounds2D(Bounds bounds) {
    return new Bounds2D {
      min = ((float3)bounds.min).xy,
      max = ((float3)bounds.max).xy,
    };
  }

  public static explicit operator Bounds(Bounds2D bounds) {
    return new Bounds {
      min = (Vector2)bounds.min,
      max = (Vector2)bounds.max,
    };
  }

}

}
