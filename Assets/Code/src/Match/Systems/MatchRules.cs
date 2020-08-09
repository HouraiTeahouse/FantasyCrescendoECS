using  HouraiTeahouse.FantasyCrescendo.Utils;
using UnityEngine;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
public class MatchRuleSystemGroup : ComponentSystemGroup {
}

public abstract class MatchRuleSystem : SystemBase {

  protected static void EndMatch() {
    Debug.Log("MATCH OVER!");
  }

}

[UpdateInGroup(typeof(MatchRuleSystemGroup))]
public class TrainingMatchRuleSystem : MatchRuleSystem {

  protected override void OnUpdate() {
    Entities.ForEach((ref PlayerComponent player) => {
      if (player.Is(PlayerFlags.HAS_DIED)) {
        player.SetFlags(PlayerFlags.HAS_RESPAWNED);
      }
    }).Schedule();
  }

}

[UpdateInGroup(typeof(MatchRuleSystemGroup))]
public class StockMatchRuleSystem : MatchRuleSystem {

  protected override void OnUpdate() {
    Entities.ForEach((ref PlayerComponent player) => {
      if (player.Is(PlayerFlags.HAS_DIED) && player.Stocks > 0) {
        player.SetFlags(PlayerFlags.HAS_RESPAWNED);
      }
    }).Schedule();
  }

}

[UpdateInGroup(typeof(MatchRuleSystemGroup))]
public class TimeMatchRuleSystem : MatchRuleSystem {

  bool _stockRuleActive;

  protected override void OnUpdate() {
    var matchState = GetSingleton<MatchState>();
    if (matchState.Time > 0) {
      matchState.Time--;
    } else {
      EndMatch();
    }
    SetSingleton(matchState);

    _stockRuleActive = World.GetOrCreateSystem<StockMatchRuleSystem>().Enabled;
    if (!_stockRuleActive) {
      Entities.ForEach((ref PlayerComponent player) => {
        if (player.Is(PlayerFlags.HAS_DIED)) {
          player.SetFlags(PlayerFlags.HAS_RESPAWNED);
        }
      }).Schedule();
    }
  }

}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MatchRuleSystemGroup))]
public class BlastZoneSystem : MatchRuleSystem {

  NativeList<Bounds2D> _bounds;

  protected override void OnCreate() {
    base.OnCreate();
    _bounds = new NativeList<Bounds2D>(Allocator.Persistent);
  }

  protected override void OnUpdate() {
    NativeArray<Bounds2D> bounds = _bounds.AsArray();
    Entities.ForEach((ref PlayerComponent player, in Translation translation) => {
      bool inBounds = false;
      for (var i = 0; i < bounds.Length; i++) {
        inBounds |= bounds[i].Contains(translation.Value.xy);
      }
      if (!inBounds) {
        player.Kill();
      }
    }).Schedule();
  }

  protected override void OnDestroy() {
    _bounds.Dispose();
  }
  
  public void AddBoundingBox(Bounds2D bounds) {
    _bounds.Add(bounds);
  }

  public void Reset() {
    _bounds.Clear();
  }

}

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
public class PlayerRespawnSystem : SystemBase {

  EndSimulationEntityCommandBufferSystem _ecbSystem;

  protected override void OnCreate() {
    base.OnCreate();
    _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
  }

  protected override void OnUpdate() {
    var ecb = _ecbSystem.CreateCommandBuffer();
    NativeArray<Translation> respawnPoints = GetEntityQuery(
      ComponentType.ReadOnly<Translation>(), 
      ComponentType.ReadWrite<RespawnPoint>())
      .ToComponentDataArray<Translation>(Allocator.TempJob);

    // FIXME(james7132): This has the potential for mulitple players to respawn at the 
    // same point. 
    Entities.ForEach((Entity entity, ref PlayerComponent player, ref Translation translation) => {
        if (player.Is(PlayerFlags.HAS_RESPAWNED)) {
          var idx = player.RNG.NextInt(respawnPoints.Length);
          translation = respawnPoints[idx];
        } else if (player.Is(PlayerFlags.HAS_DIED)) {
          // Disable players that have died without respawning.
          ecb.AddComponent<Disabled>(entity);
        }
        player.UnsetFlags(PlayerFlags.EVENT_FLAGS);
      }).Schedule();
    _ecbSystem.AddJobHandleForProducer(this.Dependency);
    CompleteDependency();
    respawnPoints.Dispose();
  }

}

}
