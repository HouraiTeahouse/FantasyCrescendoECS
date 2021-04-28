using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class SampleFrameDataSystem : SystemBase {

  protected override void OnUpdate() {
    float deltaTime = Time.fixedDeltaTime;

    var sampleJob = Entities
    .WithName("SampleFrames")
    .ForEach((ref PlayerComponent player, ref CharacterFrame frame, in PlayerCharacter character) => {
      player.StateTick++;
      bool validState = GetState(player, character, out CharacterState state);
      if (!validState) return;
      frame = state.Frames[math.min(player.StateTick, state.Frames.Length - 1)];
    }).Schedule(Dependency);

    var hitboxJob = Entities
    .WithName("UpdateHitboxes")
    .ForEach((ref Hitbox hitbox, ref HitboxState state, ref Translation translation) => {
      var player = state.Player;
      if (!HasComponent<PlayerComponent>(player) || 
          !HasComponent<PlayerCharacter>(player) || 
          !HasComponent<CharacterFrame>(player)) return;
      state.Enabled = GetComponent<CharacterFrame>(player).IsHitboxActive((int)state.ID);
      if (!state.Enabled) {
        state.PreviousPosition = null;
      }

      var playerComponent = GetComponent<PlayerComponent>(player);
      var character = GetComponent<PlayerCharacter>(player);
      bool validState = GetState(playerComponent, character, out CharacterState playerState);
      if (!validState || state.ID >= playerState.Hitboxes.Length) return;
      ref CharacterStateHitbox data = ref playerState.Hitboxes[state.ID];
      hitbox = data.Hitbox;
      translation = data.Positions[math.min(playerComponent.StateTick, data.Positions.Length - 1)];
    }).ScheduleParallel(sampleJob);

    var animationJob = Entities
    .WithName("SampleAnimations")
    .WithoutBurst()
    .ForEach((in PlayerComponent player, in PlayerCharacter character) => {
      bool validState = GetState(player, character, out CharacterState state);
      if (!validState) return;
      var graph = character.PlayableGraph;
      if (!graph.IsValid()) return;
      AnimationClipPlayable clip = state.Animation;
      clip.SetTime(player.StateTick * deltaTime);
      graph.GetOutputByType<AnimationPlayableOutput>(0).SetSourcePlayable(clip);
      character.PlayableGraph.Evaluate();
    }).ScheduleParallel(sampleJob);

    Dependency = JobHandle.CombineDependencies(animationJob, hitboxJob);
  }

  static bool GetState(in PlayerComponent player, in PlayerCharacter character, out CharacterState state) {
    state = new CharacterState();
    var stateId = player.StateID;
    if (!character.StateController.IsCreated || stateId < 0) return false;
    ref var states = ref character.StateController.Value.States;
    if (states.Length == 0) return false;
    state = ref states[stateId];
    return true;
  }

}

}
