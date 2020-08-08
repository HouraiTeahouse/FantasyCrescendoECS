using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
public class MatchRuleSystemGroup : ComponentSystemGroup {
}

public abstract class MatchRuleSystem : SystemBase {
}


[UpdateInGroup(typeof(MatchRuleSystemGroup))]
public class StockMatchRuleSystem : MatchRuleSystem {

  protected override void OnUpdate() {
    // TODO(james7132): Implement
  }

}

[UpdateInGroup(typeof(MatchRuleSystemGroup))]
public class TimeMatchRuleSystem : MatchRuleSystem {

  protected override void OnUpdate() {
    var matchState = GetSingleton<MatchState>();
    if (matchState.Time > 0) {
      matchState.Time--;
    } else {
      Debug.Log("MATCH OVER!");
    }
    SetSingleton(matchState);
  }

}

}
