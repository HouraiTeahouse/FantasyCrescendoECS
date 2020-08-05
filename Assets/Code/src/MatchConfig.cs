using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HouraiTeahouse.FantasyCrescendo.Players;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[Serializable]
public class MatchConfig {

  public const int kMaxSupportedPlayers = 8;

  static MatchConfig _currentConfig;
  public static ref MatchConfig CurrentConfig => ref _currentConfig;

  /// <summary>
  /// The ID of the stage that the match will be played on.
  /// </summary>
  public uint StageID;

  /// <summary>
  /// The number of stocks each player starts off with. If set to zero, the 
  /// match will not be a stock match.
  /// </summary>
  public uint Stocks;

  /// <summary>
  /// The amount of time the match will last for in ticks. If zero the game
  /// will not have a time limit.
  /// </summary>
  public uint Time;

  /// <summary>
  /// Individual configurations for each participating player.
  /// </summary>
  /// <remarks>
  /// Note that each player's player ID does not directly correspond with
  /// the array index for the player's config. All players in the game 
  /// with a valid configuration are assumed to be active. For example, 
  /// the player at index 1 may not be P2. Player 2 may be inactive and 
  /// the player may be P3 or P4 instead.
  /// </remarks>
  [SerializeField]
  PlayerConfig[] _playerConfigs;
  public ref PlayerConfig this[int playerId] => ref _playerConfigs[playerId];

  /// <summary>
  /// Gets the number of participating players in the game.
  /// </summary>
  public int PlayerCount => _playerConfigs.Length;

  public MatchConfig() : this(kMaxSupportedPlayers) {}
  public MatchConfig(int playerCount) : this(new PlayerConfig[playerCount]) {}
  public MatchConfig(PlayerConfig[] configs) {
    _playerConfigs = (PlayerConfig[])configs.Clone();
  }

  public bool IsLocal {
    get {
      if (_playerConfigs == null) return true;
      foreach (var config in _playerConfigs) {
        if (!config.IsLocal) return false;
      }
      return true;
    }
  }

  public bool IsValid { 
    get {
      foreach (var config in _playerConfigs) {
        if (!config.IsValid) return false;
      }
      return true;
    }
  }

}

}
