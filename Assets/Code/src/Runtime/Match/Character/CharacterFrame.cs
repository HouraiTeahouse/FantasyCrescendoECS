using System;
using Unity.Entities;
using Unity.Mathematics;
using HitboxBitfield = System.UInt64;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[Flags]
public enum FrameFlags : uint {
  /// <summary>
  /// If set, the player will be intangible to all attacks on all 
  /// of their hurtboxes for the frame.
  /// </summary>
  INTANGIBLE       = 1 << 0,

  /// <summary>
  /// If set, the player will be intangible to all attacks on all 
  /// of their hurtboxes for the frame.
  /// </summary>
  INVINCIBLE       = 1 << 1,

  /// <summary>
  /// If set, the player will be intangible to only projectile attacks.
  /// </summary>
  GRAZING          = 1 << 2,

  /// <summary>
  /// If set, the player will recieve damage, but cannot be knocked back.
  /// </summary>
  SUPER_ARMOR      = 1 << 3,

  /// <summary>
  /// If set, the player will face right at the start of the frame.
  /// </summary>
  FACE_RIGHT       = 1 << 4,

  /// <summary>
  /// If set, the player will face left at the start of the frame.
  /// </summary>
  FACE_LEFT        = 1 << 5,

  /// <summary>
  /// If set, the player will change their direction at the start 
  /// of the frame.
  /// </summary>
  CHANGE_DIRECTION = 1 << 5,
}

public struct CharacterFrame : IComponentData {

  public const int kMaxPlayerHitboxCount = sizeof(HitboxBitfield) * 8;

  public FrameFlags Flags;
  public HitboxBitfield ActiveHitboxes;
  public float2 Movement;
  public float DamageResistance;
  public float KnockbackResistance;

  public bool Is(FrameFlags flags) => (Flags & flags) != 0;
  public bool IsHitboxActive(int hitboxId) => (ActiveHitboxes & (1ul << hitboxId)) != 0;
}

}