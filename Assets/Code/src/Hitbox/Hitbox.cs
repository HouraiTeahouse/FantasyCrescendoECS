using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

public struct ScalableValue {
  public float Base;
  public float Scaling;

  public float ScaledTo(float scale) {
    return Base + scale * Scaling;
  }
}

public struct Hitbox : IComponentData {
  public float Radius;
  public float BaseHitstun;
  public bool MirrorDirection;
  public ScalableValue Damage;
  public ScalableValue KnockbackForce;
  public float KnockbackAngle;
  public ScalableValue Hitstun;
}

}