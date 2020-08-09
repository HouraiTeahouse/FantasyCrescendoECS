
namespace HouraiTeahouse.FantasyCrescendo.Utils {

public static class InputUtil {

  public static bool OutsideDeadZone(sbyte value, byte deadZone) {
    return value > deadZone || value < -deadZone;
  }

  public static bool OutsideDeadZone(in Vector2b input, byte deadZone) {
    return OutsideDeadZone(input.X, deadZone) || OutsideDeadZone(input.Y, deadZone);
  }

  public static Vector2b EnforceDeadZone(in Vector2b input, byte deadZone) {
    return new Vector2b {
      X = OutsideDeadZone(input.X, deadZone) ? input.X : (sbyte)0,
      Y = OutsideDeadZone(input.Y, deadZone) ? input.Y : (sbyte)0
    };
  }

  public static Vector2b MaxComponent(Vector2b value) {
    if (value.X > value.Y) {
      value.Y = 0;
    } else {
      value.X = 0;
    }
    return value;
  }

}

}