using HouraiTeahouse.FantasyCrescendo.Core;
using UnityEngine;
using TMPro;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class PlayerUIStocks : MonoBehaviour, IView<PlayerUIData> {

#pragma warning disable 0649
  [SerializeField] TMP_Text _excessDisplay;
  [SerializeField] GameObject[] _standardIndicators;
  [SerializeField] string _excessFormat = "{0}x";
#pragma warning restore 0649

  int? lastShownExcess;

  public void UpdateView(in PlayerUIData player) {
    if (!player.PlayerData.IsActive) {
      SetActive(_excessDisplay.gameObject, false);
      SetActive(0);
      return;
    }

    bool excess = player.PlayerData.Stocks > _standardIndicators.Length;
    SetActive(_excessDisplay.gameObject, excess);
    if (excess) {
      SetExcess(player.PlayerData.Stocks);
      SetActive(1);
    } else {
      SetActive(player.PlayerData.Stocks);
    }
  }

  void SetExcess(int stocks) {
    if (_excessDisplay == null || stocks == lastShownExcess) return;
    _excessDisplay.text = string.Format(_excessFormat, stocks);
    lastShownExcess = stocks;
  }

  void SetActive(int max) {
    for (var i = 0; i < _standardIndicators.Length; i++) {
      SetActive(_standardIndicators[i], i < max);
    }
  }

  void SetActive(GameObject gameObj, bool active) {
    if (gameObj == null) return;
    if (gameObj.activeInHierarchy != active) {
      gameObj.SetActive(active);
    }
  }

}

}