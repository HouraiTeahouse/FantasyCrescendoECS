using System;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Configs {

[CreateAssetMenu(fileName = "Debug Config", menuName = "Fantasy Crescendo/DebugConfig")]
public class DebugConfig : ScriptableObject {

  public Mesh HitboxMesh;
  public Material HitboxMaterial;
  public HitboxColor HitboxColors;

  [Serializable]
  public struct HitboxColor {
    public Color32 Offensive;
    public Color32 Damageable;
    public Color32 Invincible;
    public Color32 Intangible;
    public Color32 Grazing;
    public Color32 Shield;
    public Color32 Grab;

    public Color32 GetHitboxColor() => Offensive;
    public Color32 GetHurtboxColor(HurtboxType type) {
      switch (type) {
        case HurtboxType.DAMAGEABLE: return Damageable;
        case HurtboxType.INVINCIBLE: return Invincible;
        case HurtboxType.INTANGIBLE: return Intangible;
        case HurtboxType.SHIELD: return Shield;
        case HurtboxType.GRAZING: return Grazing;
        default: return Color.grey;
      }
    }
  }
}

}
