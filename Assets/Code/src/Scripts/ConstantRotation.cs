using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Scripts {

/// <summary>
/// Simple scripts that rotates the GameObject it's 
/// attached to at a constant rate.
/// </summary>
public class ConstantRotation : MonoBehaviour {

  public Vector3 RotationPerSecond;

  void Update() {
    transform.Rotate(RotationPerSecond * Time.deltaTime);
  }

}

}
