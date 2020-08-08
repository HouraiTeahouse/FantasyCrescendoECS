using HouraiTeahouse.FantasyCrescendo.Core;
using System;
using TMPro;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class PlayerUIId : MonoBehaviour, IView<PlayerUIData> {

#pragma warning disable 0649
  [SerializeField] TMP_Text _text;
  [SerializeField] string _format;
  [SerializeField] int _delta = 1;
#pragma warning restore 0649

  int? _id;

  public void UpdateView(in PlayerUIData player) {
    var id = player.Config.PlayerID + _delta;
    if (_id == id) return;
    if (string.IsNullOrEmpty(_format)) {
      _text.text = id.ToString();
    } else {
      _text.text = String.Format(_format, id);
    }
    _id = id;
  }

}

}
