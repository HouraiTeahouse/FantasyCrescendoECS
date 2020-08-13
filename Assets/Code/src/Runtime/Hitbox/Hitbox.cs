using System;
using Unity.Mathematics;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

[Serializable]
public struct ScalableValue {
  public float Base;
  public float Scaling;

  public float ScaledTo(float scale) {
    return Base + scale * Scaling;
  }
}

public struct HitboxState : IComponentData {
  public Entity Player;
  public int ID;
  public uint PlayerID;
  public bool Enabled;
  public float3? PreviousPosition;
}

[GenerateAuthoringComponent]
public struct Hitbox : IComponentData {
  public float Radius;
  public bool MirrorDirection;
  public ScalableValue Damage;
  public float KnockbackAngle;
  public ScalableValue KnockbackForce;
  public ScalableValue Hitstun;
}

}