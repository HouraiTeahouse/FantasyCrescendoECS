using HouraiTeahouse.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Assertions;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchManager : MonoBehaviour {

  public static MatchManager Instance { get; private set; }
  public Match CurrentMatch { get; private set; }
  public bool IsMatchRunning => CurrentMatch != null;

#pragma warning disable 0649
  [SerializeField, Tag] string _spawnPoints;
  [SerializeField, Tag] string _respawnPoints;
  [SerializeField] string _matchActionMap = "Match";
  [SerializeField] MatchConfig _config;
#pragma warning restore 0649

  void Awake() {
    Instance = this;
    _config.RandomSeed = (uint)new System.Random().Next();
    LoadingScreen.AddTask(StartMatch(new LocalDefaultMatch(_config)));
  }

  void FixedUpdate() => CurrentMatch?.Update();
  void OnDestroy() => EndMatch();

  public async Task StartMatch(Match match) {
    Assert.IsNotNull(match);
    InputManager.Instance.ForceNull()?.ChangeActiveActionMap(_matchActionMap);
    await match.Initialize(new MatchInitializationSettings {
      SpawnPoints = GetPointsFromTag(_spawnPoints),
      RespawnPoints = GetPointsFromTag(_respawnPoints),
    });
    CurrentMatch = match;
  }

  public void EndMatch() {
    CurrentMatch?.Dispose();
    CurrentMatch = null;
  }

  static Transform[] GetPointsFromTag(string tag) {
    if (tag == null) return new Transform[0];
    var gameObjects = GameObject.FindGameObjectsWithTag(tag);
    var seenNames = new HashSet<string>();
    foreach (var go in gameObjects) {
      if  (seenNames.Contains(go.name)) {
        throw new InvalidOperationException($"There are multiple objects of with the name {go.name}. This will lead to non-deterministic behaivor!");
      }
      seenNames.Add(go.name);
    }
    return gameObjects.OrderBy(go => go.name).Select(go => go.transform).ToArray();
  }

#if UNITY_EDITOR
  void OnDrawGizmos() {
    Gizmos.color = Color.blue;
    foreach (var spawnPoint in GetPointsFromTag(_spawnPoints)) {
      Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
    }
    Gizmos.color = Color.yellow;
    foreach (var respawnPoint in GetPointsFromTag(_respawnPoints)) {
      Gizmos.DrawWireSphere(respawnPoint.position, 0.25f);
    }
  }
#endif

}

}
