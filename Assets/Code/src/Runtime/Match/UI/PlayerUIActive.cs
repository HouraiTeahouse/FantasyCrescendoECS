using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

/// <summary>
/// A UI View implemenation that enables or disables objects based 
/// on the player's state.
/// </summary>
public class PlayerUIActive : MonoBehaviour, IView<PlayerUIData> {

#pragma warning disable 0649
  [SerializeField] bool UsePresence;
  [SerializeField] Object[] _objects;
  [SerializeField] bool _invert;
#pragma warning restore 0649

  public void UpdateView(in PlayerUIData player) {
    bool isActive = player.PlayerData.IsActive;
    if (UsePresence) {
      isActive = player.IsPresent;
    }
    if (_invert) isActive = !isActive;
    foreach (var obj in _objects) {
      ObjectUtility.SetActive(obj, isActive);
    }
  }


}

}