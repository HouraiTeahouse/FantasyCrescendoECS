using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace HouraiTeahouse.FantasyCrescendo.Utils {

public enum PhysicsLayers : uint {
  /// <summary>
  /// Used for hitboxes and hurtboxes.
  /// Only collides with others of the same layer.
  /// </summary>
  HITBOX          = 1 << 0,

  /// <summary>
  /// Used for characters's environment collision boxes.
  /// Collides with environment geometry and selectively
  /// other characters.
  /// </summary>
  CHARACTER       = 1 << 3,

  /// <summary>
  /// Used for the environment.
  /// </summary>
  STAGE           = 1 << 4,
}

}