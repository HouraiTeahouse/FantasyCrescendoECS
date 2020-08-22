using HouraiTeahouse.FantasyCrescendo.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Assertions;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public enum TransitionCondition : uint {
  UP              = 1 << 0,
  DOWN            = 1 << 1,
  FORWARD         = 1 << 2,
  BACKKWARD       = 1 << 3,
  NEUTRAL         = 1 << 4,
  SMASH           = 1 << 5,
  ATTACK          = 1 << 6,
  SPECIAL         = 1 << 7,
  JUMP            = 1 << 8,
  GRAB            = 1 << 9,
  SHIELD          = 1 << 10,
  BUTTON_PRESSED  = 1 << 11,
  BUTTON_RELEASED = 1 << 12,
  LEDGE_GRABBED   = 1 << 13,
  GROUNDED        = 1 << 14,
  AERIAL          = 1 << 15,
  CHARGE_MAX      = 1 << 16,
}

public struct CharacterStateHitbox {
  public Hitbox Hitbox;
  public BlobArray<Translation> Positions;
}

public struct CharacterStateTransition {
  public int TargetStateID;
  public uint TargetFrame;
  public uint MinFrame;
  public BlobArray<TransitionCondition> Conditions;
}

public struct CharacterState {
  public BlobString Name;
  public BlobArray<CharacterFrame> Frames;
  public BlobArray<CharacterStateHitbox> Hitboxes;
  public BlobArray<CharacterStateTransition> Transitions;
}

public class StateFrameData : ScriptableObject {

  public class Transition {
    public bool Enabled;
    public StateFrameData Target;
    public uint TargetFrame;
    public uint MinFrame;
    public List<TransitionCondition> Conditions;

    public CharacterStateTransition ToStateTransition(ref BlobBuilder builder,
                                                      Dictionary<StateFrameData, int> idMap) {
      var transition = new CharacterStateTransition { 
        TargetStateID = idMap[Target],
        TargetFrame = this.TargetFrame,
        MinFrame = this.MinFrame,
      };
      builder.Construct(ref transition.Conditions, Conditions.ToArray());
      return transition;
    }
  }

  public class HitboxData {
    public bool Enabled;
    public string BoundBone;
    public Translation Offset;
    public Hitbox Hitbox;
    public Translation[] BakedPositions;

    public CharacterStateHitbox ToStateHitbox(ref BlobBuilder builder) {
      var stateHitbox = new CharacterStateHitbox { Hitbox = this.Hitbox };
      builder.Construct(ref stateHitbox.Positions, BakedPositions.ToArray());
      return stateHitbox;
    }
  }

  public class Track {
    // A list of booleans
    public string Name;

    public bool Enabled;
    public FrameFlags Flags;
    public List<int> TogglePoints;
    public List<HitboxData> Hitboxes;

    public void Apply(CharacterFrame[] frames, int hitboxOffset = 0) {
      if (!Enabled) return;
      Assert.IsNotNull(frames);
      ulong hitboxMask = ((1ul << Hitboxes.Count) - 1) << hitboxOffset;
      var enabled = GetEnabledFrames(frames.Length);
      for (var i = 0; i < enabled.Count; i++) {
        if (!enabled[i]) continue;
        frames[i].Flags |= Flags;
        frames[i].ActiveHitboxes |= hitboxMask;
      }
    }

    public List<bool> GetEnabledFrames(int length) {
      var enabled = new List<bool>(new bool[length]);
      if (TogglePoints == null) return enabled;
      TogglePoints.Sort();
      var idx = 0;
      var value = false;
      foreach (var point in TogglePoints) {
        while (idx < point && idx < enabled.Count) {
          enabled[idx] = value;
        }
        value = !value;
      }
      return enabled;
    }

    public void SetEnabledFrames(bool[] track) {
      TogglePoints = new List<int>();
      if (track == null || track.Length <= 0) return;
      var value = false;
      for (var i = 0; i < track.Length; i++) {
        if (track[i] != value) {
          TogglePoints.Add(i);
        }
        value = !value;
      }
    }

  }

#pragma warning disable 0649
  [SerializeField] string _name;
  [SerializeField] int _length;
  [SerializeField] List<Transition> _transitions;

  public AnimationClip Animation;
  public List<Track> Tracks;

  [SerializeField] AnimationCurve HorizontalMovement;
  [SerializeField] AnimationCurve VerticalMovement;
  [SerializeField] AnimationCurve DamageResistance;
  [SerializeField] AnimationCurve KnockbackResistance;
#pragma warning restore 0649

  public CharacterState BuildState(ref BlobBuilder builder, 
                                   Dictionary<StateFrameData, int> idMap) {
    var state = new CharacterState();
    builder.AllocateString(ref state.Name, _name);
    builder.Construct(ref state.Frames, BuildFrames());
    builder.Construct(ref state.Hitboxes, BuildHitboxes(ref builder));

    var transitions = new List<CharacterStateTransition>();
    foreach (var transition in _transitions) {
      if (!transition.Enabled) continue;
      transitions.Add(transition.ToStateTransition(ref builder, idMap));
    }
    builder.Construct(ref state.Transitions, transitions.ToArray());
    return state;
  }

  CharacterFrame[] BuildFrames() {
    var frames = new CharacterFrame[_length];
    var hitboxOffset = 0;
    foreach (var track in Tracks) {
      if (!track.Enabled) continue;
      track?.Apply(frames, hitboxOffset);
      hitboxOffset += (track?.Enabled ?? false) ? track.Hitboxes.Count : 0;
    }

    for (var i = 0; i < frames.Length; i++) {
      var time = ((float)i)/frames.Length;
      frames[i].Movement.x = HorizontalMovement.Evaluate(time);
      frames[i].Movement.y = VerticalMovement.Evaluate(time);
      frames[i].DamageResistance = DamageResistance.Evaluate(time);
      frames[i].KnockbackResistance = KnockbackResistance.Evaluate(time);
    }
    return frames;
  }

  CharacterStateHitbox[] BuildHitboxes(ref BlobBuilder builder) {
    var hitboxes = new List<CharacterStateHitbox>();
    foreach (var track in Tracks) {
      if (!track.Enabled) continue;
      foreach (var hitbox in track.Hitboxes) {
        if (!hitbox.Enabled) continue;
        hitboxes.Add(hitbox.ToStateHitbox(ref builder));
        if (hitboxes.Count >= CharacterFrame.kMaxPlayerHitboxCount) {
          return hitboxes.ToArray();
        }
      }
    }
    return hitboxes.ToArray();
  }

}

}