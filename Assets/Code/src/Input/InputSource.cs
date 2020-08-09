using HouraiTeahouse.FantasyCrescendo.Matches;
using UnityEngine.InputSystem;
using Unity.Collections;
using Unity.Entities;
using PlayerInput = HouraiTeahouse.FantasyCrescendo.Matches.PlayerInput;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public interface InputSource {
  void AdvanceFrame();
  PlayerInput GetLocalInput(int localPlayerId);
}

public class DefaultInputSource : InputSource {

  public void AdvanceFrame() {}

  public PlayerInput GetLocalInput(int localPlayerId) {
    // TODO(james7132): Map this somehow
    return new PlayerInput();
  }

}

public class ReplayInputSource : InputSource {

  public void AdvanceFrame() {}

  public PlayerInput GetLocalInput(int localPlayerId) {
    return new PlayerInput();
  }

}

}