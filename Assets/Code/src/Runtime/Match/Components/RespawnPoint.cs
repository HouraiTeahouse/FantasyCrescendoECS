using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct RespawnPoint : IComponentData {
  public bool IsOccupied => Occupant != null;
  public Entity? Occupant;
}

}