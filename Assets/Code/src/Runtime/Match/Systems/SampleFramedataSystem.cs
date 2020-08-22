using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class SampleFrameDataSystem : SystemBase {

  protected override void OnUpdate() {
    Entities
    .WithName("SampleNames")
    .ForEach((ref PlayerComponent player, ref CharacterFrame frame) => {
      player.StateTick++;
      bool validState = GetState(player, out CharacterState state);
      if (!validState) return;
      frame = state.Frames[math.min(player.StateTick, state.Frames.Length - 1)];
    }).Schedule();

    Entities
    .WithName("UpdateHitboxes")
    .ForEach((ref Hitbox hitbox, ref HitboxState state, ref Translation translation) => {
      var player = state.Player;
      if (!HasComponent<PlayerComponent>(player) || !HasComponent<CharacterFrame>(player)) return;
      state.Enabled = GetComponent<CharacterFrame>(player).IsHitboxActive((int)state.ID);
      if (!state.Enabled) {
        state.PreviousPosition = null;
      }

      var playerComponent = GetComponent<PlayerComponent>(player);
      bool validState = GetState(playerComponent, out CharacterState playerState);
      if (!validState || state.ID >= playerState.Hitboxes.Length) return;
      ref CharacterStateHitbox data = ref playerState.Hitboxes[state.ID];
      hitbox = data.Hitbox;
      translation = data.Positions[math.min(playerComponent.StateTick, data.Positions.Length - 1)];
    }).ScheduleParallel();
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
