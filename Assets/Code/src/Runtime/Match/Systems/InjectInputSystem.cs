using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class InjectInputsSystem : SystemBase {

  NativeArray<PlayerInput> _inputs;

  protected override void OnCreate() {
    _inputs = MatchConfig.CreateNativePlayerBuffer<PlayerInput>();
  }

  protected override void OnUpdate() {
    NativeArray<PlayerInput> inputs = _inputs;
    Entities.ForEach((ref PlayerInputComponent input, in PlayerConfig config) => {
      input.Update(inputs[config.PlayerID]);
    }).Schedule();
  }

  protected override void OnDestroy() {
    _inputs.Dispose();
  }

  public void SetPlayerInput(int playerId, in PlayerInput input) {
    Assert.IsTrue(playerId >= 0 && playerId < MatchConfig.kMaxSupportedPlayers);
    CompleteDependency();
    _inputs[playerId] = input;
  }

}

}