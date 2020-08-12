using System;
using HouraiTeahouse.FantasyCrescendo.Utils;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct PlayerInput {

  [Flags]
  public enum ButtonBits {
    ATTACK, SPECIAL, JUMP, SHIELD, GRAB
  }

  public byte Buttons;
  public Vector2b Movement;
  public Vector2b Smash;

  public bool Attack {
    get => BitUtil.GetBit(Buttons, (int)ButtonBits.ATTACK);
    set => BitUtil.SetBit(ref Buttons, (int)ButtonBits.ATTACK, value);
  }

  public bool Special {
    get => BitUtil.GetBit(Buttons, (int)ButtonBits.SPECIAL);
    set => BitUtil.SetBit(ref Buttons, (int)ButtonBits.SPECIAL, value);
  }

  public bool Jump {
    get => BitUtil.GetBit(Buttons, (int)ButtonBits.JUMP);
    set => BitUtil.SetBit(ref Buttons, (int)ButtonBits.JUMP, value);
  }

  public bool Shield {
    get => BitUtil.GetBit(Buttons, (int)ButtonBits.SHIELD);
    set => BitUtil.SetBit(ref Buttons, (int)ButtonBits.SHIELD, value);
  }

  public bool Grab {
    get => BitUtil.GetBit(Buttons, (int)ButtonBits.GRAB);
    set => BitUtil.SetBit(ref Buttons, (int)ButtonBits.GRAB, value);
  }

  public static bool operator ==(PlayerInput a, PlayerInput b) {
    var equals = (a.Buttons & 31) == (b.Buttons & 31);
    equals &= a.Movement == b.Movement;
    equals &= a.Smash == b.Smash;
    return equals;
  }

  public static bool operator !=(PlayerInput a, PlayerInput b) => !(a == b);

  public override bool Equals(object other) {
    return (other is PlayerInput) && ((PlayerInput)other) == this;
  }

  public override string ToString() {
    var buttons = Convert.ToString(Buttons, 2).PadLeft(8, '0');
    return $"PlayerInput(({Movement.x}, {Movement.y}), ({Smash.x}, {Smash.y}), {buttons})";
  }

  public override int GetHashCode() => 
    31 * Movement.GetHashCode() + 
    17 * Smash.GetHashCode() + 
    (Buttons & 31).GetHashCode();

}

}