using HouraiTeahouse.FantasyCrescendo.Core;
using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchPlayerUIDisplay : MonoBehaviour {

#pragma warning disable 0649
  [SerializeField] RectTransform _displayPrefab;
  [SerializeField] RectTransform _displayContainer;
  [SerializeField] RectTransform _indicatorPrefab;
  [SerializeField] RectTransform _indicatorContainer;
#pragma warning restore 0649

  IView<PlayerUIData>[] _indicators;
  IView<PlayerUIData>[] _playerUIViews;

  void Awake() {
    _indicators = CreateViews(_indicatorPrefab, _indicatorContainer);
    _playerUIViews = CreateViews(_displayPrefab, _displayContainer);
  }

  void LateUpdate() {
    var displaySystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerUIDisplaySystem>();
    for (var i = 0; i < _playerUIViews.Length; i++) {
      var playerData = displaySystem.GetPlayerUIData(i);
      _indicators[i]?.UpdateView(playerData);
      _playerUIViews[i]?.UpdateView(playerData);
    }
  }

  IView<PlayerUIData>[] CreateViews(RectTransform prefab, RectTransform container) {
    var store = MatchConfig.CreatePlayerBuffer<IView<PlayerUIData>>();
    if (prefab == null) return store;
    for (var i = 0; i < store.Length; i++) {
      RectTransform playerView = GameObject.Instantiate(prefab);
      if (container != null) {
        playerView.parent = container;
      }
      playerView.gameObject.name = playerView.gameObject.name.Replace("(Clone)", "") + " " + (i + 1);
      store[i] = AggregateView<PlayerUIData>.FromGameObject(playerView.gameObject);
    }
    return store;
  }

}

}