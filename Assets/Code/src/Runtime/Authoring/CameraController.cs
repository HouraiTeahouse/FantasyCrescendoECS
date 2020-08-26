using UnityEngine;
using Unity.Mathematics;

namespace HouraiTeahouse.FantasyCrescendo.Authoring {
  
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

  public Vector3 PositionBias;
  public Vector2 FoVRange;
  public float SmoothTime = 0.5f;
  public float WidthPadding = 5f;
  public float MaxSpreadDistance = 50f;

  Camera _camera;
  Vector3 velocity;

  void Awake() {
    _camera = GetComponent<Camera>();
  }

  public void Move(Bounds bounds) {
    var center = bounds.center + PositionBias;
    var width = bounds.size.x + WidthPadding;

    center.z = transform.position.z;
    transform.position = Vector3.SmoothDamp(transform.position, center, ref velocity, SmoothTime);

    _camera.fieldOfView = Mathf.Lerp(FoVRange.x, FoVRange.y, width / MaxSpreadDistance);
  }

}

}
