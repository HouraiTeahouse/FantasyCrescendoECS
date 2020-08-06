using HouraiTeahouse.FantasyCrescendo.Utils;
using UnityEngine;
using Unity.Entities;
using Unity.Entities.Serialization;

namespace HouraiTeahouse.FantasyCrescendo {

public class MatchManager : MonoBehaviour {

  DynamicBinaryWriter _stateWriter;

  void Awake() {
    _stateWriter = new DynamicBinaryWriter(1024);
  }

  void Update() {
    var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    SerializeUtility.SerializeWorld(manager, _stateWriter);
    _stateWriter.Trim();
    _stateWriter.Reset(1024);
  }

  void OnDestroy() {
    _stateWriter?.Dispose();
  }

}

}
