using HouraiTeahouse.FantasyCrescendo.Core;
using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchPlayerUIDisplay : MonoBehaviour {

#pragma warning disable 0649
  [SerializeField] GameObject _displayPrefab;
  [SerializeField] Transform _container;
#pragma warning restore 0649

  IView<PlayerUIData>[] _playerUIViews;

  void Awake() {
    _playerUIViews = MatchConfig.CreatePlayerBuffer<IView<PlayerUIData>>();
    if (_displayPrefab == null) return;

    for (var i = 0; i < _playerUIViews.Length; i++) {
      GameObject playerView = GameObject.Instantiate(_displayPrefab);
      if (_container != null) {
        playerView.transform.parent = _container;
      }
      _playerUIViews[i] = AggregateView<PlayerUIData>.FromGameObject(playerView);
    }
  }

  void LateUpdate() {
    var displaySystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerUIDisplaySystem>();
    for (var i = 0; i < _playerUIViews.Length; i++) {
      _playerUIViews[i]?.UpdateView(displaySystem.GetPlayerUIData(i));
    }
  }

}

}