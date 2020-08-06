using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

public struct TimeToLive : IComponentData {
  /// <summary>
  // The total number of ticks the entity will live for. The tick
  // this reaches zero, it'll be deleted by the DestroyTemporaryEntities
  // system at the end of the tick.
  /// </summary>
  public uint FramesRemaining;
}

}