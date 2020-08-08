using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

public struct PlayerComponent : IComponentData {
  public bool IsActive => Stocks > 0;
  public bool IsRespawning => RespawnTimeRemaining > 0;
  public bool IsHit => Hitstun > 0;

  // TODO(james7132): Change this into a bit field
  public bool Direction;
  public bool IsFastFalling;
  public bool IsTeetering;

  public int Stocks;
  public float Damage;
  public int Hitstun;
  public int Hitlag;

  public uint StateID;
  public uint StateTick;

  public byte Charge;

  public ushort ShieldDamage;
  public ushort ShieldRecoveryCooldown;

  public uint RespawnTimeRemaining;
}

}