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
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchManager : MonoBehaviour {

  World _world;
  SimulationSystemGroup _simulation;
  DynamicBinaryWriter _stateWriter;
  BlobAssetStore _blobAssetStore;

  public static MatchManager Instance { get; private set; }
  public bool IsMatchRunning { get; private set; }

#pragma warning disable 0649
  [SerializeField, Tag] string _spawnPoints;
  [SerializeField, Tag] string _respawnPoints;
  [SerializeField] string _matchActionMap = "Match";
  [SerializeField] MatchConfig _config;
#pragma warning restore 0649

  void Awake() {
    Instance = this;

    _stateWriter = new DynamicBinaryWriter(1024);
    _world = World.DefaultGameObjectInjectionWorld;
    _simulation = _world.GetOrCreateSystem<SimulationSystemGroup>();
    _simulation.Enabled = false;
    _simulation.SortSystems();

    _config.RandomSeed = (uint)new System.Random().Next();

    LoadingScreen.AddTask(StartMatch());
  }

  void FixedUpdate() {
    _world.PushTime(new TimeData(Time.fixedTime, Time.fixedDeltaTime));
    _simulation.Enabled = true;
    SampleLocalInputs();
    _simulation.Update();
    _simulation.Enabled = false;
    _world.PopTime();
  }

  void Update() {
    SerializeUtility.SerializeWorld(_world.EntityManager, _stateWriter);
    _stateWriter.Reset(1024);
  }

  void OnDestroy() {
    _stateWriter?.Dispose();
  }

  void SampleLocalInputs() {
    var manager = InputManager.Instance;
    var system = _world?.GetOrCreateSystem<InjectInputsSystem>();
    if (manager == null || system == null) return;
    for (var i = 0; i < _config.PlayerCount; i++) {
      if (!_config[i].IsLocal) continue;
      var sampledInput = manager.GetInputForPlayer(_config[i].LocalPlayerID);
      system.SetPlayerInput(_config[i].PlayerID, sampledInput);
    }
  }

  async Task StartMatch() {
    InputManager.Instance.ForceNull()?.ChangeActiveActionMap(_matchActionMap);
    await Task.WhenAll(SpawnPlayers());
    _world.EntityManager.CreateEntity(ComponentType.ReadOnly<MatchState>());
    _world.GetExistingSystem<SimulationSystemGroup>().SetSingleton(_config.CreateInitialState());

    var enabledRules = new HashSet<Type>(MakeMatchRules());
    foreach (var system in _world.GetExistingSystem<MatchRuleSystemGroup>().Systems) {
      if (system.Enabled = enabledRules.Contains(system.GetType())) {
        Debug.Log($"Enabled match rule: {system}");
      }
    }
    IsMatchRunning = true;
  }

  IEnumerable<Type> MakeMatchRules() {
    if (_config.Time > 0) {
      yield return typeof(TimeMatchRuleSystem);
    }
    if (_config.Stocks > 0) {
      yield return typeof(StockMatchRuleSystem);
    }
  }

  async Task SpawnPlayers() {
    await DataLoader.WaitUntilLoaded();
    SetupStage();
    _blobAssetStore = new BlobAssetStore();
    var spawnPoints = GetPointsFromTag(_spawnPoints);
    var tasks = new Task<GameObject>[_config.PlayerCount];
    for (var i = 0; i < _config.PlayerCount; i++) {
      Vector3 spawnPoint = spawnPoints[i % spawnPoints.Length];
      tasks[i] = LoadPlayerGameObject(_config[i], spawnPoint);
    }
    var playerGos = await Task.WhenAll(tasks);
    for (var i = 0; i < playerGos.Length; i++) {
      SpawnPlayer(_config[i], playerGos[i]);
    }
    Debug.Log("Players spawned!");
  }

  async Task<GameObject> LoadPlayerGameObject(PlayerConfig config, Vector3 position) {
    var pallete = config.Selection.GetPallete();
    var prefab = await pallete.Prefab.LoadAssetAsync<GameObject>().Task;
    var player = GameObject.Instantiate(prefab, position, Quaternion.identity);
#if UNITY_EDITOR
    player.name = $"Player {config.PlayerID + 1} ({prefab.name})";
#endif
    return player;
  }

  void SpawnPlayer(PlayerConfig config, GameObject player) {
    var settings = new GameObjectConversionSettings(_world, 
                        GameObjectConversionUtility.ConversionFlags.AssignName, 
                        _blobAssetStore);
    var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(player, settings);
    _world.EntityManager.AddComponentData(entity, config);
    _world.EntityManager.AddComponentData(entity, new PlayerComponent {
      RNG = new Unity.Mathematics.Random((uint)(_config.RandomSeed ^ (1 << config.PlayerID))),
      Stocks = (int)_config.Stocks,
      Damage = config.DefaultDamage,
    });
    Destroy(player);
    Debug.Log($"Player {config.PlayerID} spawned!");
  }

  void SetupStage() {
    var respawnPoints = GetPointsFromTag(_respawnPoints);
    var entityManager = _world.EntityManager;
    var archetype = entityManager.CreateArchetype(
      ComponentType.ReadWrite<Translation>(),
      ComponentType.ReadWrite<RespawnPoint>()
    );
    foreach (var point in respawnPoints) {
      var entity = entityManager.CreateEntity(archetype);
      entityManager.AddComponentData(entity, new Translation { Value = point });
    }
  }

  static Vector3[] GetPointsFromTag(string tag) {
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
