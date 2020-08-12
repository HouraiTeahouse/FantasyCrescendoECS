using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class LocalDefaultMatch : Match {

  public LocalDefaultMatch(MatchConfig config, World world = null) : base(config, world) {
  }

  protected override IEnumerable<Type> GetRuleTypes() {
    Assert.IsNotNull(Config);
    if (Config.Time > 0) {
      yield return typeof(TimeMatchRuleSystem);
    }
    if (Config.Stocks > 0) {
      yield return typeof(StockMatchRuleSystem);
    }
  }

}

}