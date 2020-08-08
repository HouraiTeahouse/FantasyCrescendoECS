using System;
using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

[Flags]
public enum PlayerFlags : byte {
  FACING_LEFT   = 1 << 0,
  GROUNDED      = 1 << 1,
  FAST_FALLING  = 1 << 2,
  TEETERING     = 1 << 3,
}

public struct PlayerComponent : IComponentData {

  // 12 bytes
  public PlayerFlags Flags;                   // 1 byte
  public float Damage;                        // 4 bytes
  [SerializeField] byte _stateID;             // 1 bytes
  [SerializeField] byte _stateTick;           // 1 byte
  [SerializeField] sbyte _stocks;             // 1 byte
  [SerializeField] byte _hitstun;             // 1 byte
  [SerializeField] byte _hitlag;              // 1 byte
  [SerializeField] byte _charge;              // 1 byte

  public int Stocks {
    get => (int)_stocks;
    set => _stocks = (sbyte)value;
  }
  public int Hitstun {
    get => (int)_hitstun;
    set => _hitstun = (byte)value;
  }
  public int Hitlag {
    get => (int)_hitlag;
    set => _hitlag = (byte)value;
  }

  public int StateID {
    get => (int)_stateID;
    set => _stateID = (byte)value;
  }
  public int StateTick {
    get => (int)_stateTick;
    set => _stateTick = (byte)value;
  }

  public bool IsActive => Stocks > 0;
  public bool IsRespawning => RespawnTimeRemaining > 0;
  public bool IsHit => Hitstun > 0;

  public bool Is(PlayerFlags flagMask) => (Flags & flagMask) != 0;

  public ushort ShieldDamage;
  public ushort ShieldRecoveryCooldown;

  public ushort RespawnTimeRemaining;
}

}