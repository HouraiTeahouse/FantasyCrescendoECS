using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public enum Direction {
  Neutral, Left, Right, Up, Down
}

public struct Vector2b {
  public sbyte X;
  public sbyte Y;

  public float x  {
    get => ToFloat(X);
    set => X = FromFloat(value);
  }

  public float y  {
    get => ToFloat(Y);
    set => Y = FromFloat(value);
  }

  public Direction Direction {
    get {
      Vector2b dir = InputUtil.EnforceDeadZone(this, 39);
      sbyte absX = abs(dir.X);
      sbyte absY = abs(dir.Y);
      if (absX > absY) {
        if (dir.X < 0) return Direction.Left;
        if (dir.X > 0) return Direction.Right;
      }
      if (absX <= absY) {
        if (dir.Y < 0) return Direction.Down;
        if (dir.Y > 0) return Direction.Up;
      }
      return Direction.Neutral;

      sbyte abs(sbyte x) {
        return (sbyte)(x < 0 ? x : -x);
      }
    }
  }

  public static explicit operator Vector2b(Vector2 vector) => new Vector2b { x = vector.x, y = vector.y };
  public static explicit operator Vector2(Vector2b vector) => new Vector2 { x = vector.x, y = vector.y };

  float ToFloat(sbyte val) => (float)val / sbyte.MaxValue;
  sbyte FromFloat(float val) => (sbyte)(Mathf.Clamp(val, -1, 1) * sbyte.MaxValue);

  public static bool operator ==(Vector2b a, Vector2b b) => a.X == b.X && a.Y == b.Y;
  public static bool operator !=(Vector2b a, Vector2b b) => !(a == b);

  public override bool Equals(object obj) {
    return (obj is Vector2b) && ((Vector2b)obj) == this;
  }

  public override int GetHashCode() => unchecked(31 * X + Y);
  public override string ToString() => $"Vector2b<{(Vector2)this}>";

}

}