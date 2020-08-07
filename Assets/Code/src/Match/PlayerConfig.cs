using System;
using HouraiTeahouse;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

/// <summary>
/// A data object for configuring a single player within a multiplayer match.
/// </summary>
[Serializable]
public struct PlayerConfig : IComponentData {

  /// <summary>
  /// Is the player's config valid for starting a game?
  /// If any player's config is invalid, a game cannot be started.
  /// </summary>
  public bool IsValid => Selection.IsValid;

  /// <summary>
  /// Is the player playing locally to the current machine?
  /// </summary>
  public bool IsLocal => LocalPlayerID >= 0;

  /// <summary>
  /// The Player ID of the player. Determines what is visually displayed
  /// to denote the player.
  /// </summary>
  public byte PlayerID;

  /// <summary>
  /// The local player number. Mainly used to determine what local input 
  /// device to read the input from.
  /// </summary>
  public sbyte LocalPlayerID;

  /// <summary>
  /// The player's selection of what character to play.
  /// </summary>
  public PlayerSelection Selection;

  /// <summary>
  /// The default amount of damage the player will have on (re)spawning.
  /// </summary>
  public float DefaultDamage;

  public override string ToString() => Selection.ToString();

}

/// <summary>
/// A data object for managing the human selected elements of a player's
/// configuration.
/// </summary>
[Serializable]
public struct PlayerSelection {

  [GameDataId(typeof(Character))]
  public uint CharacterID;            // 1-4 bytes
  public byte Pallete;                // 1 byte
  
  /// <summary>
  /// Is the player's selection valid for starting a game?
  /// If any player's config is invalid, a game cannot be started.
  /// </summary>
  public bool IsValid {
    get {
      var character = Registry.Get<Character>().Get(CharacterID);
      if (character == null) return false;
      return Pallete < character.Palletes.Length;
    }
  }

  public Character GetCharacter() => Registry.Get<Character>().Get(CharacterID);
  public Character.Pallete GetPallete() => GetCharacter()?.Palletes[Pallete];

  public override string ToString() => $"Selection({CharacterID},{Pallete})";

}


}