using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HouraiTeahouse.FantasyCrescendo {

[CreateAssetMenu(menuName = "Fantasy Crescendo/SceneData (Stage)")]
public class SceneData : GameData {

  public SceneType Type = SceneType.Stage;
  public string Name;

  public AssetReference Scene;
  public AssetReference Icon;
  public AssetReference PreviewImage;

  public int LoadPriority;

  // TODO(james7132): Add BGM handling back in
  //public BGM[] Music;

  public override string ToString() => $"Scene ({name})";
}

public enum SceneType {
  Menu, Stage
}

}