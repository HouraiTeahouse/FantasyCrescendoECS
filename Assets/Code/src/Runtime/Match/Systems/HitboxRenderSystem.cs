using HouraiTeahouse.FantasyCrescendo.Configs;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class HitboxRenderSystem : SystemBase {

  struct HitboxRenderData {
    public Color32 Color;
    public Matrix4x4 Transform;
  }

  // Keyed by height
  NativeMultiHashMap<float, HitboxRenderData> _renderData;
  MaterialPropertyBlock _block;
  Dictionary<Color32, List<Matrix4x4>> _transforms;

  protected override void OnCreate() {
    _transforms = new Dictionary<Color32, List<Matrix4x4>>();
    _block = new MaterialPropertyBlock();
    _renderData = new NativeMultiHashMap<float, HitboxRenderData>(
      MatchConfig.kMaxSupportedPlayers * CharacterFrame.kMaxPlayerHitboxCount,
      Allocator.Persistent
    );
  }

  protected override void OnUpdate() {
    var config = Config.Get<DebugConfig>();
    _renderData.Clear();
    var writer = _renderData.AsParallelWriter();
    DebugConfig.HitboxColor colors = config.HitboxColors;

    var hitboxJob = Entities
    .WithBurst(FloatMode.Fast)
    .ForEach((in Hitbox hitbox, in HitboxState state, in LocalToWorld transform) => {
      if (!state.Enabled) return;
      float3 currentPosition = math.transform(transform.Value, float3.zero);
      float3 prevPosition = state.PreviousPosition ?? currentPosition;
      float3 center = (currentPosition + prevPosition) / 2;
      float height = math.length(prevPosition - currentPosition) / hitbox.Radius;
      var scale = new float3(hitbox.Radius);

      var trs = (Matrix4x4)float4x4.TRS(center, quaternion.identity, scale);
      writer.Add(height, new HitboxRenderData { Color = colors.GetHitboxColor(), Transform = trs });
    }).ScheduleParallel(Dependency);

    // var hurtboxJob = Entities
    // .WithBurst(FloatMode.Fast)
    // .WithNativeDisableContainerSafetyRestriction(writer)
    // .ForEach((in Hurtbox hitbox, in LocalToWorld transform) => {
    //   float3 currentPosition = math.transform(transform.Value, float3.zero);
    //   float3 prevPosition = state.PreviousPosition ?? currentPosition;
    //   float3 center = (currentPosition + prevPosition) / 2;
    //   float height = math.length(prevPosition - currentPosition);
    //   var scale = new float3(1);

    //   var trs = (Matrix4x4)matrix.TRS(center, new float4(0), scale);
    //   writer.Add(new HitboxMaterial { Color = colors.GetHitboxColor(), Height = height, }, trs);
    // }).ScheduleParallel(Dependency);

    // Dependency = JobHandle.CombineDependencies(hitboxJob, hurtboxJob);
    Dependency = hitboxJob;
    CompleteDependency();

    var (keys, length) = _renderData.GetUniqueKeyArray(Allocator.Temp);
    for (var i = 0; i < length; i++) {
      RenderBatch(keys[i], config);
    }
  }

  protected override void OnDestroy() {
    _renderData.Dispose();
  }

  void RenderBatch(float height, DebugConfig config) {
    if (config.HitboxMesh == null || config.HitboxMaterial == null) return;
    foreach (var value in _transforms.Values) {
      value.Clear();
    }
    var iter = _renderData.GetValuesForKey(height);
    while (iter.MoveNext()) {
      List<Matrix4x4> transforms;
      if (!_transforms.TryGetValue(iter.Current.Color, out transforms)) {
        transforms = new List<Matrix4x4>();
        _transforms[iter.Current.Color] = transforms;
      }
      transforms.Add(iter.Current.Transform);
    }
    iter.Dispose();
    _block.SetFloat("_Height", height);
    foreach (var kvp in _transforms) {
      if (kvp.Value.Count < 0) continue;
      _block.SetColor("_Color", kvp.Key);
      Graphics.DrawMeshInstanced(
        /*mesh=*/config.HitboxMesh, 
        /*submeshIndex=*/0, 
        /*material=*/config.HitboxMaterial, 
        /*matricies=*/kvp.Value, 
        /*properties*/_block, 
        /*castShadows=*/ShadowCastingMode.Off, 
        /*recieveShadows=*/false);
    }
  }

}

}