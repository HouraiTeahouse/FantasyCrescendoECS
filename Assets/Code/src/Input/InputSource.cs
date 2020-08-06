using HouraiTeahouse.FantasyCrescendo.Matches;
using UnityEngine.InputSystem;
using Unity.Collections;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Players {

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