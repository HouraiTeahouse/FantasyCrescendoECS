using System;
using HouraiTeahouse.FantasyCrescendo.Utils;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct PlayerInput {

  [Flags]
  public enum Button : byte {
    ATTACK, SPECIAL, JUMP, SHIELD, GRAB
  }

  public Button Buttons;
  public Vector2b Movement;
  public Vector2b Smash;

  public bool IsPressed(Button button) => (Buttons & button) != 0;
  public void Set(Button button, bool value) {
    Buttons = value ? (Buttons | button) : (Buttons & ~button);
  }

  public static bool operator ==(PlayerInput a, PlayerInput b) {
    var equals = ((int)a.Buttons & 31) == ((int)b.Buttons & 31);
    equals &= a.Movement == b.Movement;
    equals &= a.Smash == b.Smash;
    return equals;
  }

  public static bool operator !=(PlayerInput a, PlayerInput b) => !(a == b);

  public override bool Equals(object other) {
    return (other is PlayerInput) && ((PlayerInput)other) == this;
  }

  public override string ToString() {
    var buttons = Convert.ToString((byte)Buttons, 2).PadLeft(8, '0');
    return $"PlayerInput(({Movement.x}, {Movement.y}), ({Smash.x}, {Smash.y}), {buttons})";
  }

  public override int GetHashCode() => 
    31 * Movement.GetHashCode() + 
    17 * Smash.GetHashCode() + 
    ((byte)Buttons & 31).GetHashCode();

}

}