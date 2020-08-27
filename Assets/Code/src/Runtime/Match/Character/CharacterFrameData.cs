using HouraiTeahouse.FantasyCrescendo.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct CharacterStateController {
  public BlobArray<CharacterState> States;
}

public class CharacterFrameData : ScriptableObject {

#pragma warning disable 0649
  [SerializeField] StateFrameData _default;
  public List<StateFrameData> States;
#pragma warning restore 0649

  BlobAssetReference<CharacterStateController> _reference;

  public BlobAssetReference<CharacterStateController> BuildController(CharacterControllerBuildParams builderParams) {
    if (_reference != BlobAssetReference<CharacterStateController>.Null) return _reference;
    var builder = new BlobBuilder(Allocator.Temp);
    ref CharacterStateController controller = ref builder.ConstructRoot<CharacterStateController>();
    builderParams.StateMap = BuildIdMap(States);
    var actions = States?.Select(a => a.BuildState(ref builder, builderParams));
    builder.Construct(ref controller.States, actions);
    _reference = builder.CreateBlobAssetReference<CharacterStateController>(Allocator.Persistent);
    builder.Dispose();
    return _reference;
  }

  void OnDisable() {
    _reference.Dispose();
  }

  static Dictionary<T, int> BuildIdMap<T>(IList<T> list) {
    var map = new Dictionary<T, int>();
    for (var i = 0; i < (list?.Count ?? 0); i++) {
      map[list[i]] = i;
    }
    return map;
  }
  
}

}