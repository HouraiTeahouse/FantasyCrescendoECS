using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class SampleFrameDataSystem : SystemBase {

  protected override void OnUpdate() {
    var entityManager = World.EntityManager;

    var hitboxes = GetComponentDataFromEntity<Hitbox>(false);
    var hitboxStates = GetComponentDataFromEntity<HitboxState>(false);
    var positions = GetComponentDataFromEntity<Translation>(false);
    var scales = GetComponentDataFromEntity<Scale>(false);

    Entities.ForEach((Entity entity, ref PlayerComponent player, ref CharacterFrame frame) => {
      player.StateTick++;
      bool validState = GetState(player, out CharacterState state);
      if (!validState) return;
      frame = state.Frames[math.min(player.StateTick, state.Frames.Length - 1)];
      var playerHitboxes = entityManager.GetBuffer<PlayerHitboxBuffer>(entity);
      for (var i = 0; i < playerHitboxes.Length; i++) {
        Entity hitbox = playerHitboxes[i].Hitbox;

        var hitboxState = hitboxStates[hitbox];
        hitboxState.Enabled = frame.IsHitboxActive(i);
        hitboxStates[hitbox] = hitboxState;

        if (i >= state.Hitboxes.Length) continue;
        hitboxes[hitbox] = state.Hitboxes[i].Hitbox;
        positions[hitbox] = state.Hitboxes[i].Translation;
        scales[hitbox] = state.Hitboxes[i].Scale;
      }
    }).Schedule();

    CompleteDependency();
  }

  static bool GetState(in PlayerComponent player, out CharacterState state) {
    state = new CharacterState();
    var stateId = player.StateID;
    if (!player.StateController.IsCreated || stateId < 0) return false;
    ref var states = ref player.StateController.Value.States;
    if (states.Length == 0) return false;
    state = ref states[stateId];
    return true;
  }

}

}
