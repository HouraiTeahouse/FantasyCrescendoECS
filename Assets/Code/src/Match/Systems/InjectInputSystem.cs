using HouraiTeahouse.FantasyCrescendo.Players;
using Unity.Collections;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class InjectInputsSystem : SystemBase {

  NativeArray<PlayerInput> _inputs;

  protected override void OnCreate() {
    _inputs = MatchConfig.CreateNativePlayerBuffer<PlayerInput>();
  }

  protected override void OnUpdate() {
    NativeArray<PlayerInput> sampledInputs = _inputs;
    Entities.ForEach((ref PlayerInputComponent input, in PlayerConfig config) => {
      input.Update(sampledInputs[config.PlayerID]);
    }).Schedule();
  }

  protected override void OnDestroy() {
    _inputs.Dispose();
  }

}

}