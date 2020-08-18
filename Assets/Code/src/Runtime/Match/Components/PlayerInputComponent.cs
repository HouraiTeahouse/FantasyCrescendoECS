using System;
using Unity.Entities;
using UnityEngine;
using HouraiTeahouse;
using HouraiTeahouse.FantasyCrescendo.Utils;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct PlayerInputComponent : IComponentData {

  public PlayerInput Current;
  public PlayerInput Previous;

  public bool IsPressed(PlayerInput.Button button) => Current.IsPressed(button);
  public bool WasPressed(PlayerInput.Button button) => Current.IsPressed(button) && !Previous.IsPressed(button);
  public bool WasRelease(PlayerInput.Button button) => !Current.IsPressed(button) && Previous.IsPressed(button);

  public void Update(in PlayerInput input) {
    Previous = Current;
    Current = input;
  }

}

}