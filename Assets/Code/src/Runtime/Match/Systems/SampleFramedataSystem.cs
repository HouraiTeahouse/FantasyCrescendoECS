using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class SampleFrameDataSystem : SystemBase {

  protected override void OnUpdate() {
    var entityManager = World.EntityManager;

    var players = GetComponentDataFromEntity<PlayerComponent>(true);
    var frames = GetComponentDataFromEntity<CharacterFrame>(true);

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
    .WithReadOnly(players)
    .WithReadOnly(frames)
    .ForEach((ref Hitbox hitbox, ref HitboxState state, ref Translation translation, ref Scale scale) => {
      var player = state.Player;
      if (!players.HasComponent(player) || !frames.HasComponent(player)) return;
      state.Enabled = frames[state.Player].IsHitboxActive((int)state.ID);

      bool validState = GetState(players[player], out CharacterState playerState);
      if (!validState || state.ID >= playerState.Hitboxes.Length) return;
      var data = playerState.Hitboxes[state.ID];
      hitbox = data.Hitbox;
      translation = data.Translation;
      scale = data.Scale;
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
