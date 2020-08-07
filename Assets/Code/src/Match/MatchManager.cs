using HouraiTeahouse.Attributes;
using HouraiTeahouse.FantasyCrescendo.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Core;
using Unity.Entities;
using Unity.Entities.Serialization;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchManager : MonoBehaviour {

  World _world;
  SimulationSystemGroup _simulation;
  DynamicBinaryWriter _stateWriter;

#pragma warning disable 0649
  [SerializeField, Tag] string _spawnPoints;
  [SerializeField, Tag] string _respawnPoints;
  [SerializeField] MatchConfig _config;
#pragma warning restore 0649

  void Awake() {
    _stateWriter = new DynamicBinaryWriter(1024);
    _world = World.DefaultGameObjectInjectionWorld;
    _simulation = _world.GetOrCreateSystem<SimulationSystemGroup>();
    _simulation.Enabled = false;
    _simulation.SortSystems();
    LoadingScreen.AddTask(SpawnPlayers());
  }

  void FixedUpdate() {
    _world.PushTime(new TimeData(Time.fixedTime, Time.fixedDeltaTime));
    _simulation.Enabled = true;
    _simulation.Update();
    _simulation.Enabled = false;
    _world.PopTime();
  }

  void Update() {
    SerializeUtility.SerializeWorld(_world.EntityManager, _stateWriter);
    _stateWriter.Trim();
    _stateWriter.Reset(1024);
  }

  void OnDestroy() {
    _stateWriter?.Dispose();
  }

  async Task SpawnPlayers() {
    await DataLoader.WaitUntilLoaded();
    var spawnPoints = GetPointsFromTag(_spawnPoints);
    var tasks = new Task[_config.PlayerCount];
    for (var i = 0; i < _config.PlayerCount; i++) {
      Vector3 spawnPoint = spawnPoints[i % spawnPoints.Length];
      tasks[i] = SpawnPlayer(_config[i], spawnPoint);
    }
    await Task.WhenAll(tasks);
    Debug.Log("Players spawned!");
  }

  async Task SpawnPlayer(PlayerConfig config, Vector3 position) {
    var settings = new GameObjectConversionSettings(_world, 
                        GameObjectConversionUtility.ConversionFlags.AssignName);
    var pallete = config.Selection.GetPallete();
    var prefab = await pallete.Prefab.LoadAssetAsync<GameObject>().Task;
    var player = GameObject.Instantiate(prefab, position, Quaternion.identity);
    var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(player, settings);
    _world.EntityManager.AddComponentData(entity, config);
    Destroy(player);
    Debug.Log($"Player {config.PlayerID} spawned!");
  }

  Vector3[] GetPointsFromTag(string tag) {
    if (tag == null) return new Vector3[0];
    var gameObjects = GameObject.FindGameObjectsWithTag(tag);
    var seenNames = new HashSet<string>();
    foreach (var go in gameObjects) {
      if  (seenNames.Contains(go.name)) {
        throw new InvalidOperationException($"There are multiple objects of with the name {go.name}. This will lead to non-deterministic behaivor!");
      }
      seenNames.Add(go.name);
    }
    return gameObjects.OrderBy(go => go.name).Select(go => go.transform.position).ToArray();
  }

#if UNITY_EDITOR
  void OnDrawGizmos() {
    Gizmos.color = Color.blue;
    foreach (var spawnPoint in GetPointsFromTag(_spawnPoints)) {
      Gizmos.DrawWireSphere(spawnPoint, 0.25f);
    }
    Gizmos.color = Color.yellow;
    foreach (var respawnPoint in GetPointsFromTag(_respawnPoints)) {
      Gizmos.DrawWireSphere(respawnPoint, 0.25f);
    }
  }
#endif

}

}
