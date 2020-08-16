using System;
using System.Globalization;
using UnityEngine;
using TMPro;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public class PllayerUICharacterName : MonoBehaviour, IView<PlayerUIData> {

  uint? _characterId;

  [SerializeField] TMP_Text _text;
  [SerializeField] NameChoice _name;
  [SerializeField] Case _caseChange;

  enum NameChoice {
    Short, Long
  }

  enum Case {
    None, Upper, Lower, Title
  }

  public void UpdateView(in PlayerUIData player) {
    var playerCharacterId = player.Config.Selection.CharacterID;
    if (_characterId == playerCharacterId) return;
    var character = Registry.Get<Character>().Get(playerCharacterId);
    if (character == null) return;
    _text.text = UpdateCase(GetName(character));
    _characterId = playerCharacterId;
  }

  string GetName(Character character) {
    switch (_name) {
      case NameChoice.Short: 
        return character.ShortName;
      case NameChoice.Long: 
        return character.LongName;
      default:
        throw new InvalidOperationException();
    }
  }

  string UpdateCase(string input) {
    switch (_caseChange) {
      case Case.Upper: 
        return input.ToUpperInvariant();
      case Case.Lower: 
        return input.ToLowerInvariant();
      case Case.Title: 
        return new CultureInfo("en-US", false).TextInfo.ToTitleCase(input);
      default:
        return input;
    }
  }

}

}