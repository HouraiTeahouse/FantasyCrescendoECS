using HouraiTeahouse.FantasyCrescendo.Core;
using UnityEngine;
using TMPro;

namespace HouraiTeahouse.FantasyCrescendo.Matches.UI {

/// <summary>
/// A UI view that allows displaying a players current damage.
/// </summary>
public class PlayerUIDamage : MonoBehaviour, IView<PlayerUIData> {

  public const float kMaxDisplayDamage = 999.99f;

  public TMP_Text DisplayText;
  public string Format;
  public Gradient DisplayColor;
  public float MinDamage;
  public float MaxDamage;

  float? lastDamage;

  public void UpdateView(in PlayerUIData player) {
    if (DisplayText == null) {
      Debug.LogWarning($"{name} has a PlayerDamage without a Text display.");
      return;
    }
    SetTextDamage(Mathf.Min(player.PlayerData.Damage, kMaxDisplayDamage));
  }

  void SetTextDamage(float damage) {
    damage = Mathf.Floor(damage);
    if (damage == lastDamage) return;
    var displayDamage = Mathf.Round(damage);
    if (string.IsNullOrEmpty(Format)) {
      DisplayText.text = displayDamage.ToString();
    } else {
      DisplayText.text = string.Format(Format, displayDamage);
    }
    DisplayText.color = GetColor(damage);
    lastDamage = damage;
  }

  Color GetColor(float damage) {
    var interp = Mathf.InverseLerp(MinDamage, MaxDamage, damage);
    interp = Mathf.Clamp01(interp);
    return DisplayColor.Evaluate(interp);
  }

}

}
