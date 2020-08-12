using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
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
    }).Schedule();
    _ecbSystem.AddJobHandleForProducer(this.Dependency);
  }

}

}