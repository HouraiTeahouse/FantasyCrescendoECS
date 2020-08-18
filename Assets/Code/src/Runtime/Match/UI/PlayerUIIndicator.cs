using UnityEngine;
using UnityEngine.UI;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class PlayerUIIndicator : MonoBehaviour, IView<PlayerUIData> {

  public Vector3 PositionBias = new Vector3(0, 1, 0);

  Camera _camera;
  RectTransform CanvasTransform => gameObject.transform.parent as RectTransform;

  public void UpdateView(in PlayerUIData player) {
    if (_camera == null) {
      _camera = Camera.main;
      if (_camera == null) return;
    }
    Vector3 worldPosition = player.WorldPosition + PositionBias;
    Vector2 viewportPosition = _camera.WorldToViewportPoint(worldPosition);
    Vector2 canvasSize = CanvasTransform.rect.size;
    Vector2 position = Vector2.Scale(viewportPosition, canvasSize) - 0.5f * canvasSize;
    (transform as RectTransform).anchoredPosition = position;
  }

}

}