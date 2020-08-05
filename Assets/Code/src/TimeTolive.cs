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

public class DestroyTemporaryEntities : SystemBase {

    EndSimulationEntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate() {
      base.OnCreate();
      _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
      var ecb = _ecbSystem.CreateCommandBuffer().ToConcurrent();
      Entities.ForEach((Entity entity, int entityInQueryIndex, ref TimeToLive ttl) => {
        if (ttl.FramesRemaining == 0) {
          ecb.DestroyEntity(entityInQueryIndex, entity);
        } else {
          ttl.FramesRemaining--;
        }
      }).ScheduleParallel();

      _ecbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

}