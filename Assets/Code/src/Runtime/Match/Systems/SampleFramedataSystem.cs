using Unity.Mathematics;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
public class SampleFrameDataSystem : SystemBase {

  BeginSimulationEntityCommandBufferSystem _ecbSystem;

  protected override void OnCreate() {
    base.OnCreate();
    _ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
  }

  protected override void OnUpdate() {
    var entityManager = World.EntityManager;
    var ecb = _ecbSystem.CreateCommandBuffer();

    Entities.ForEach((Entity entity, ref PlayerComponent player, ref CharacterFrame frameData) => {
      player.StateTick++;
      CharacterFrame? frame = GetFrame(player);
      if (frame == null) return;
      frameData = frameData;
      var hitboxes = entityManager.GetBuffer<PlayerHitboxBuffer>(entity);
      for (var i = 0; i < hitboxes.Length; i++) {
        if (frameData.IsHitboxActive(i)) {
          ecb.RemoveComponent<Disabled>(hitboxes[i].Hitbox);
        } else  {
          ecb.AddComponent<Disabled>(hitboxes[i].Hitbox);
        }
      }
    }).Schedule();
    _ecbSystem.AddJobHandleForProducer(this.Dependency);
    CompleteDependency();
  }

  static CharacterFrame? GetFrame(in PlayerComponent player) {
    var stateId = player.StateID;
    if (!player.StateController.IsCreated || stateId < 0) return null;
    ref var states = ref player.StateController.Value.States;
    if (states.Length == 0) return null;
    ref var frames = ref states[stateId].Frames;
    if (frames.Length == 0) return null;
    return frames[math.min(player.StateTick, frames.Length - 1)];
  }

}

}
