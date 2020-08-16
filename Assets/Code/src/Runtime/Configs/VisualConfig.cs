using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Configs {

[CreateAssetMenu(fileName = "Fantasy Crescendo/Config/VisualConfig")]
public class VisualConfig : ScriptableObject {

#pragma warning disable 0649
  [SerializeField] Color[] _playerColors = new[] {
    Color.red, Color.blue, Color.yellow, Color.green
  };
#pragma warning restore 0649

  public Color GetPlayerColor(int playerId) {
    return _playerColors?[playerId % _playerColors.Length] ?? Color.grey;
  }

}

}