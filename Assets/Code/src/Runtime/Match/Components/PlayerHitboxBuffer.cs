using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[InternalBufferCapacity(64)]
public struct PlayerHitboxBuffer : IBufferElementData {
  public Entity Hitbox;
}

}