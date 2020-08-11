using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class TrainingMatch : Match {

  public TrainingMatch(MatchConfig config, World world = null) : base(config, world) {
  }

  protected override IEnumerable<Type> GetRuleTypes() {
    Assert.IsNotNull(Config);
    yield return typeof(TrainingMatchRuleSystem);
  }

}

}
