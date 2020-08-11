using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class InjectInputsSystem : SystemBase {

  PlayerInput[] _inputs;

  protected override void OnCreate() {
    _inputs = MatchConfig.CreatePlayerBuffer<PlayerInput>();
  }

  protected override void OnUpdate() {
    var sampledInputs = new NativeArray<PlayerInput>(_inputs.Length, Allocator.TempJob);
    sampledInputs.CopyFrom(_inputs);
    Entities.ForEach((ref PlayerInputComponent input, in PlayerConfig config) => {
      input.Update(sampledInputs[config.PlayerID]);
    }).Schedule();
    Dependency = sampledInputs.Dispose(Dependency);
  }

  public void SetPlayerInput(int playerId, in PlayerInput input) {
    Assert.IsTrue(playerId >= 0 && playerId < MatchConfig.kMaxSupportedPlayers);
    _inputs[playerId] = input;
  }

}

}