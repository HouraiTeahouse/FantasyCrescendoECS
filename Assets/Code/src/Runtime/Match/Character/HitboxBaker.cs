using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class HitboxBaker : IDisposable {

  readonly GameObject _gameObject;  
  readonly Animator _animator;

  public HitboxBaker(GameObject prefab) {
    _gameObject = GameObject.Instantiate(prefab);
    _animator = _gameObject.GetComponentInChildren<Animator>();
  }

  public void BakeController(CharacterFrameData frameData) {
    if (frameData == null || frameData.States == null) return;
    foreach (var state in frameData.States) {
      BakeState(state);
    }
  }

  public void BakeState(StateFrameData frameData) {
    Dictionary<string, List<StateFrameData.HitboxData>> hitboxes = BuildHitboxMap(frameData);
    Dictionary<string, Transform> bones = GetBoneMap(_gameObject);
    var frameCount = GetFrameCount(frameData.Animation);
    foreach (var hitbox in hitboxes.Values.SelectMany(x => x)) {
      hitbox.BakedPositions = new Translation[frameCount];
    }

    if (hitboxes.Count < 0 || bones.Count < 0) return;
    var root = _gameObject.transform;
    var graph = PlayableGraph.Create();
    var output = AnimationPlayableOutput.Create(graph, "Animation", _animator);
    var playable = AnimationClipPlayable.Create(graph, frameData.Animation);
    for (var i = 0; i < frameCount; i++) {
      playable.SetTime(i * Time.fixedDeltaTime);
      graph.Evaluate();
      foreach (var kvp in hitboxes) {
        Transform bind;
        if (!bones.TryGetValue(kvp.Key, out bind)) {
          bind = root;
        }

        foreach (var hitbox in kvp.Value) {
          var worldPos = bind.TransformPoint(hitbox.Offset.Value);
          hitbox.BakedPositions[i].Value = root.InverseTransformPoint(worldPos);
        }
      }
    }
#if UNITY_EDITOR
    EditorUtility.SetDirty(frameData);
#endif
    graph.Destroy();
  }

  public void Dispose() {
    if (_gameObject != null) {
      UnityEngine.Object.DestroyImmediate(_gameObject);
    }
  }

  static int GetFrameCount(AnimationClip clip) {
    if (clip == null) return 0;
    return Mathf.FloorToInt(clip.length / Time.fixedDeltaTime);
  }

  static Dictionary<string, List<StateFrameData.HitboxData>> BuildHitboxMap(StateFrameData state) {
    var hitboxes = new Dictionary<string, List<StateFrameData.HitboxData>>();
    foreach (StateFrameData.Track track in state.Tracks) {
      foreach (var hitbox in track.Hitboxes) {
        var key = hitbox.BoundBone ?? "";
        List<StateFrameData.HitboxData> data;
        if (!hitboxes.TryGetValue(key, out data)) {
          data = new List<StateFrameData.HitboxData>();
          hitboxes[key] = data;
        }
        data.Add(hitbox);
      }
    }
    return hitboxes;
  }

  static Dictionary<string, Transform> GetBoneMap(GameObject root) {
    var transforms = new Dictionary<string, Transform>();
    var children = root.GetComponentsInChildren<Transform>();
    foreach (var child in children) {
      if (transforms.ContainsKey(child.name)) {
        Debug.LogWarning($"Rig has multiple bones with the name '{child.name}'! This could cause issues with binding.");
      }
      transforms[child.name] = child;
    }
    return transforms;
  }

}

}