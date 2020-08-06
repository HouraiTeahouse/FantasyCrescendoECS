using HouraiTeahouse.FantasyCrescendo.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Core;
using Unity.Entities;
using Unity.Entities.Serialization;

namespace HouraiTeahouse.FantasyCrescendo {

public class MatchManager : MonoBehaviour {

  World _world;
  SimulationSystemGroup _simulation;
  DynamicBinaryWriter _stateWriter;

  void Awake() {
    _stateWriter = new DynamicBinaryWriter(1024);
    _world = World.DefaultGameObjectInjectionWorld;
    _simulation = _world.GetOrCreateSystem<SimulationSystemGroup>();
    _simulation.Enabled = false;
    _simulation.SortSystems();
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

}

}
