using HouraiTeahouse.FantasyCrescendo.Utils;
using HouraiTeahouse.FantasyCrescendo.Authoring;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Core;
using Unity.Collections;
using Unity.Transforms;
using Unity.Assertions;
using Unity.Mathematics;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchInitializationSettings {
  public Transform[] SpawnPoints;
  public Transform[] RespawnPoints;
}

public abstract class Match : IDisposable {

  public MatchConfig Config { get; protected set; }

  protected World World { get; }
  protected EntityManager EntityManager => World.EntityManager;
  protected ComponentSystemGroup Simulation { get; }

  BlobAssetStore _blobAssetStore;

  protected Match(MatchConfig config, World world = null) {
    Config = config;
    World = world ?? Unity.Entities.World.DefaultGameObjectInjectionWorld;

    Simulation = World.GetOrCreateSystem<SimulationSystemGroup>();
    Simulation.Enabled = false;
    Simulation.SortSystems();
    
    _blobAssetStore = new BlobAssetStore();
  }

  public async Task Initialize(MatchInitializationSettings settings) {
    SetupMatchRules();
    SetupStage(settings);

    EntityManager.CreateEntity(ComponentType.ReadOnly<MatchState>());
    Simulation.SetSingleton(Config.CreateInitialState());

    await SpawnPlayers(settings);
  }

  protected void Step() {
    World.PushTime(new TimeData(Time.fixedTime, Time.fixedDeltaTime));
    Simulation.Enabled = true;
    SampleInputs();
    Simulation.Update();
    Simulation.Enabled = false;
    World.PopTime();
  }

  public virtual void Update() {
    var hasher = World.GetOrCreateSystem<HashWorldSystem>();
    var id = WorldPool.Instance.Get(out World saveState);
    saveState.EntityManager.CopyAndReplaceEntitiesFrom(EntityManager);
    hasher.Update();
    ulong[] beforeHash = hasher.GetComponentHashes();
    Step();
    ulong[] hash = hasher.GetComponentHashes();
    EntityManager.CopyAndReplaceEntitiesFrom(saveState.EntityManager);
    hasher.Update();
    ulong[] beforeHash2 = hasher.GetComponentHashes();
    Step();
    ulong[] hash2 = hasher.GetComponentHashes();
    for (var i = 0; i < hash2.Length; i++) {
      if (beforeHash[i] == beforeHash2[i]) continue;
      Debug.Log($"BEFORE Component Difference at {i}: {beforeHash[i]} {beforeHash2[i]}");
    }
    for (var i = 0; i < hash2.Length; i++) {
      if (hash[i] == hash2[i]) continue;
      Debug.Log($"AFTER Component Difference at {i}: {hash[i]} {hash2[i]}");
    }
    WorldPool.Instance.Remove(id);
  }
  public virtual void Dispose() {
    _blobAssetStore?.Dispose();
    _blobAssetStore = null;
  }

  protected abstract IEnumerable<Type> GetRuleTypes();

  protected virtual void SampleInputs() {
    var manager = InputManager.Instance;
    if (manager == null) return;
    var inputs = MatchConfig.CreateNativePlayerBuffer<PlayerInput>(Allocator.Temp);
    for (var i = 0; i < Config.PlayerCount; i++) {
      if (!Config[i].IsLocal) continue;
      inputs[Config[i].PlayerID] = manager.GetInputForPlayer(Config[i].LocalPlayerID);
    }
    InjectInputs(inputs);
  }

  protected void InjectInputs(NativeArray<PlayerInput> inputs) {
    var system = World?.GetOrCreateSystem<InjectInputsSystem>();
    for (var i = 0; i < inputs.Length; i++) {
      system.SetPlayerInput(i, inputs[i]);
    }
  }

  void SetupMatchRules() {
    var enabledRules = new HashSet<Type>(GetRuleTypes());
    foreach (var system in World.GetExistingSystem<MatchRuleSystemGroup>().Systems) {
      if (system.Enabled = enabledRules.Contains(system.GetType())) {
        Debug.Log($"Enabled match rule: {system}");
      }
    }
  }

  void SetupStage(MatchInitializationSettings settings) {
    var system  = World.GetExistingSystem<PlayerRespawnSystem>();
    system.Reset();
    foreach (var point in settings.RespawnPoints) {
      system.AddRespawnPoint(point.position);
    }
  }

  async Task SpawnPlayers(MatchInitializationSettings settings) {
    await DataLoader.WaitUntilLoaded();
    var spawnPoints = settings.SpawnPoints;
    var tasks = new Task<GameObject>[Config.PlayerCount];
    for (var i = 0; i < Config.PlayerCount; i++) {
      Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
      tasks[i] = LoadPlayerGameObject(Config[i], spawnPoint);
    }
    var playerGos = await Task.WhenAll(tasks);
    for (var i = 0; i < playerGos.Length; i++) {
      ToPlayerEntity(Config[i], playerGos[i]);
    }
    Debug.Log("Players spawned!");
  }

  async Task<GameObject> LoadPlayerGameObject(PlayerConfig config, Transform transform) {
    var pallete = config.Selection.GetPallete();
    var prefab = await pallete.Prefab.LoadAssetAsync<GameObject>().Task;
    var player = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
#if UNITY_EDITOR
    player.name = $"Player {config.PlayerID + 1} ({prefab.name})";
#endif
    var playerComponent = ObjectUtil.GetOrAddComponent<Player>(player);
    playerComponent.PlayerID = config.PlayerID;
    return player;
  }

  Entity ToPlayerEntity(PlayerConfig playerConfig, GameObject player) {
    var settings = new GameObjectConversionSettings(World, 
                        GameObjectConversionUtility.ConversionFlags.AssignName, 
                        _blobAssetStore);
    var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(player, settings);
    EntityManager.AddComponentData(entity, playerConfig);
    EntityManager.AddComponentData(entity, new PlayerComponent {
      RNG = new Unity.Mathematics.Random((uint)(Config.RandomSeed ^ (1 << playerConfig.PlayerID))),
      Stocks = (int)Config.Stocks,
      Damage = playerConfig.DefaultDamage,
    });
    UnityEngine.Object.Destroy(player);
    Debug.Log($"Player {playerConfig.PlayerID} spawned!");
    return entity;
  }

}

}