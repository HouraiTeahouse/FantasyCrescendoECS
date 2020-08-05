using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HouraiTeahouse.Attributes;
using HouraiTeahouse.FantasyCrescendo.Utils;

namespace HouraiTeahouse.FantasyCrescendo.Players {

/// <summary>
/// A data object representing a playable character.
/// </summary>
[CreateAssetMenu(menuName = "Fantasy Crescendo/Character")]
public class Character : GameData {

  [Serializable]
  public class Pallete {

    public SpriteReference Portrait;
    public GameObjectReference Prefab;

    public void Unload() {
      Portrait.ReleaseAsset();
      Prefab.ReleaseAsset();
    }

  }

  public string ShortName;
  public string LongName;

  public SceneDataReference HomeStage;
  public AudioClipReference VictoryTheme;
  [Header("Visuals")]
  public SpriteReference Icon;
  public Pallete[] Palletes;
  public Vector2 PortraitCropCenter;
  public float PortraitCropSize;

  public Rect PortraitCropRect {
    get {
      var size = Vector2.one * PortraitCropSize;
      var extents = size / 2;
      return new Rect(PortraitCropCenter - extents, size);
    }
  }

  public void Unload() {
    Icon.ReleaseAsset();
    HomeStage.ReleaseAsset();
    VictoryTheme.ReleaseAsset();
  }

  public override string ToString() => $"Character ({name})";

}

}