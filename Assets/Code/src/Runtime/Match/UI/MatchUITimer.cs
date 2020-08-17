using TMPro;
using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class MatchUITimer : MonoBehaviour {

#pragma warning disable 0649
  [SerializeField] TMP_Text _text;
#pragma warning restore 0649

  int? _seconds;

  void LateUpdate() {
    if (!MatchManager.Instance.IsMatchRunning) return;
    var em = World.DefaultGameObjectInjectionWorld.EntityManager;
    var query = em.CreateEntityQuery(ComponentType.ReadOnly<MatchState>());
    var ticks = query.GetSingleton<MatchState>().Time;
    if (ticks == 0) {
      ObjectUtility.SetActive(_text, false);
      return;
    }
    var seconds = Mathf.FloorToInt(ticks * Time.fixedDeltaTime);
    if (_seconds == seconds) return;
    _seconds = seconds;
    var minutes = seconds / 60;
    seconds = seconds % 60;
    _text.text = $"{minutes:D2}:{seconds:D2}";
    ObjectUtility.SetActive(_text, true);
  }

}

}