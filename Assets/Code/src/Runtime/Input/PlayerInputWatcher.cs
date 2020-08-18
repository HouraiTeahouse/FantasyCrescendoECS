using HouraiTeahouse.FantasyCrescendo.Configs;
using HouraiTeahouse.FantasyCrescendo.Utils;
using System.Collections.Generic;
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

  InputAction movementAction;
  InputAction strongAction;

  Dictionary<PlayerInput.Button, InputAction> _buttons;

  void Awake() {
    Assert.IsNotNull(_inputSource);
    var index = _inputSource.playerIndex;
    FindObjectOfType<InputManager>().ForceNull()?.Register(index, this);
    Debug.Log($"Local player {index} started.");
    gameObject.name = gameObject.name.Replace("(Clone)", "") + " " + index.ToString();

    InputActionAsset actions =_inputSource.actions;
    movementAction = actions[_movementAction];
    strongAction = actions[_strongAction];

    _buttons = new Dictionary<PlayerInput.Button, InputAction>();
    _buttons[PlayerInput.Button.ATTACK] = actions[_attackAction];
    _buttons[PlayerInput.Button.SPECIAL] = actions[_shieldAction];
    _buttons[PlayerInput.Button.JUMP] = actions[_jumpAction];
    _buttons[PlayerInput.Button.GRAB] = actions[_grabAction];
    _buttons[PlayerInput.Button.SHIELD] = actions[_shieldAction];
  }

  void OnDestroy() {
    FindObjectOfType<InputManager>().ForceNull()?.Unregister(_inputSource.playerIndex);
  }

  public PlayerInput GetLatestInputs() {
    var buttons = (PlayerInput.Button)0;
    foreach (var kvp in _buttons) {
      if (kvp.Value.ReadValue<bool>()) {
        buttons |= kvp.Key;
      }
    }
    return new PlayerInput {
      Buttons = buttons,
      Movement = (Vector2b)movementAction.ReadValue<Vector2>(),
      Smash    = (Vector2b)strongAction.ReadValue<Vector2>(),
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