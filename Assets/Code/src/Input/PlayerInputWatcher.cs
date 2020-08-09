using HouraiTeahouse.FantasyCrescendo.Core;
using HouraiTeahouse.FantasyCrescendo.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using PlayerInput = HouraiTeahouse.FantasyCrescendo.Matches.PlayerInput;
using PlayerInputBehaviour = UnityEngine.InputSystem.PlayerInput;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[RequireComponent(typeof(PlayerInputBehaviour))]
public class PlayerInputWatcher : MonoBehaviour {

  class TapInputDetector {

    static InputConfig InputConfig;

    public Vector2b LastRawInput;
    public Vector2b CurrentRawInput;

    public Vector2b SmashValue;
    public int SmashFramesRemaining;

    public TapInputDetector() {
      if (InputConfig == null) {
        InputConfig = Config.Get<InputConfig>();
      }
    }

    public void Update(Vector2 newInput) {
      LastRawInput = CurrentRawInput;
      CurrentRawInput = (Vector2b)newInput;

      SmashFramesRemaining = Mathf.Max(0, SmashFramesRemaining - 1);

      var lastInput = InputUtil.EnforceDeadZone(LastRawInput, InputConfig.DeadZone);
      var currentInput = InputUtil.EnforceDeadZone(CurrentRawInput, InputConfig.DeadZone);
      if (InputUtil.OutsideDeadZone(lastInput, InputConfig.DeadZone)) {
        SmashValue = Vector2b.zero;
        return;
      } 

      var diff = currentInput - lastInput;
      diff = InputUtil.EnforceDeadZone(diff, InputConfig.SmashThreshold);
      diff = InputUtil.MaxComponent(diff);

      if (SmashFramesRemaining > 0) {
        // Has recently smashed, needs to be in a different direction to change
        var currentDirection = SmashValue.Direction;
        var newDirection = diff.Direction;
        if (currentDirection != newDirection) {
          RefreshSmashValue(diff);
        }
      } else if (!InputUtil.OutsideDeadZone(diff, InputConfig.SmashThreshold)) {
        SmashValue = Vector2b.zero;
      } else {
        RefreshSmashValue(diff);
      }
    }

    void RefreshSmashValue(Vector2b value) {
      SmashValue = value;
      SmashFramesRemaining = (int)InputConfig.SmashFrameWindow;
    }

  }

#pragma warning disable 0649
  [SerializeField] PlayerInputBehaviour _inputSource;
  [SerializeField] string _movementAction = "Match/Move";
  [SerializeField] string _strongAction   = "Match/Strong";
  [SerializeField] string _attackAction   = "Match/Attack";
  [SerializeField] string _specialAction  = "Match/Special";
  [SerializeField] string _jumpAction     = "Match/Jump";
  [SerializeField] string _grabAction     = "Match/Grab";
  [SerializeField] string _shieldAction   = "Match/Shield";
#pragma warning restore 0649

  void Start() {
    Assert.IsNotNull(_inputSource);
    var index = _inputSource.playerIndex;
    FindObjectOfType<InputManager>().ForceNull()?.Register(index, this);
    Debug.Log($"Local player {index} started.");
    gameObject.name = gameObject.name.Replace("(Clone)", "") + " " + index.ToString();
  }

  void OnDestroy() {
    FindObjectOfType<InputManager>().ForceNull()?.Unregister(_inputSource.playerIndex);
  }

  public PlayerInput GetLatestInputs() {
    InputActionAsset actions =_inputSource.actions;
    return new PlayerInput {
      Movement = (Vector2b)actions[_movementAction].ReadValue<Vector2>(),
      Smash    = (Vector2b)actions[_strongAction].ReadValue<Vector2>(),
      Attack   = actions[_attackAction].ReadValue<bool>(),
      Special  = actions[_specialAction].ReadValue<bool>(),
      Jump     = actions[_jumpAction].ReadValue<bool>(),
      Grab     = actions[_grabAction].ReadValue<bool>(),
      Shield   = actions[_shieldAction].ReadValue<bool>(),
    };
  }

  InputAction GetAction(string key) {
    return _inputSource.actions[key];
  }

  // TODO(james7132): Uncomment when match pausing/resetting is ready.
  // public void OnPause(InputAction.CallbackContext value) {
  //   var matchManager = MatchManager.Instance;
  //   if (matchManager == null) return;
  //   matchManager.TogglePaused(_inputSource.playerIndex);
  // }

  // public void OnReset(InputAction.CallbackContext value) {
  //   var matchManager = MatchManager.Instance;
  //   if (matchManager == null) return;
  //   matchManager.ResetMatch(_inputSource.playerIndex);
  // }

}

}