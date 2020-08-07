using HouraiTeahouse.FantasyCrescendo.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HouraiTeahouse.FantasyCrescendo.Matches.UI {

/// <summary>
/// A UI view for changing a Unity UI component colors to match that
/// of a given player. The color is defined by the registered VisualConfig.
/// </summary>
public class PlayerUIColor : MonoBehaviour, IView<PlayerUIData> {

#pragma warning disable 0649
  [SerializeField] Graphic[] _graphics;
#pragma warning restore 0649

  public void UpdateView(in PlayerUIData player) {
    var color = Config.Get<VisualConfig>().GetPlayerColor(player.Config.PlayerID);
    foreach (var graphic in _graphics) {
      if (graphic == null) continue;
      graphic.color = color;
    }
  }

}

}