using HouraiTeahouse.FantasyCrescendo.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct CharacterStateTransition {
  public int TargetStateID;
}

public struct CharacterState {
  public BlobString Name;
  public BlobArray<CharacterFrame> Frames;
  public BlobArray<CharacterStateTransition> Transitions;
}

public class StateFrameData : ScriptableObject {

  public class Transition {
    public StateFrameData Target;

    public CharacterStateTransition ToStateTransition(Dictionary<StateFrameData, int> idMap) {
      return new CharacterStateTransition { TargetStateID = idMap[Target] };
    }
  }

#pragma warning disable 0649
  [SerializeField] string _name;
  [SerializeField] List<CharacterFrame> _frames;
  [SerializeField] List<Transition> _transitions;
#pragma warning restore 0649

  public CharacterState BuildState(ref BlobBuilder builder, 
                                   Dictionary<StateFrameData, int> idMap) {
    var state = new CharacterState();
    var transitions = _transitions?.Select(t => t.ToStateTransition(idMap));
    builder.AllocateString(ref state.Name, _name);
    builder.Construct(ref state.Frames, _frames?.ToArray() ?? new CharacterFrame[0]);
    builder.Construct(ref state.Transitions, transitions ?? new CharacterStateTransition[0]);
    return state;
  }

}

}