using Unity.Entities;
using Unity.Mathematics;

namespace HouraiTeahouse.FantasyCrescendo {

public struct MatchState : IComponentData {

  /// <summary>
  /// Represents the match's state in game
  /// </summary>
  public enum ProgressionState { 
    /// <summary>
    /// Before the game begins. Represents the countdown sequence.
    /// </summary>
    Intro, 
    /// <summary>
    /// Represents the game in progress.
    /// </summary>
    InGame, 
    /// <summary>
    /// Represents the game while paused. Can only be paused during InGame.
    /// </summary>
    Pause, 
    /// <summary>
    /// After the game results are in. Represents the GAME/TIME sequence.
    /// </summary>
    End 
  }

  /// <summary>
  /// How much time, in ticks, remains in the match.
  /// </summary>
  public uint Time;

  public ProgressionState StateID;

  /// <summary>
  /// The global RNG state.
  /// </summary>
  public Random Random;

}

}