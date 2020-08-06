using System;
using Unity.Entities;
using UnityEngine;
using HouraiTeahouse;
using HouraiTeahouse.FantasyCrescendo.Utils;

namespace HouraiTeahouse.FantasyCrescendo.Players {

public struct PlayerInputComponent : IComponentData {

  public PlayerInput Current;
  public PlayerInput Previous;

  public Button Attack => new Button(Previous.Buttons, Current.Buttons, 
                                     (int)PlayerInput.ButtonBits.ATTACK);
  public Button Special => new Button(Previous.Buttons, Current.Buttons, 
                                      (int)PlayerInput.ButtonBits.SPECIAL);
  public Button Jump => new Button(Previous.Buttons, Current.Buttons, 
                                   (int)PlayerInput.ButtonBits.JUMP);
  public Button Shield => new Button(Previous.Buttons, Current.Buttons, 
                                     (int)PlayerInput.ButtonBits.SHIELD);
  public Button Grab => new Button(Previous.Buttons, Current.Buttons, 
                                   (int)PlayerInput.ButtonBits.GRAB);

  public readonly struct Button {

    public readonly bool Previous;
    public readonly bool Current;

    public Button(byte previous, byte current, int bit) {
      Previous = BitUtil.GetBit(previous, bit);
      Current = BitUtil.GetBit(current, bit);
    }

    public bool WasPressed => !Previous && Current;
    public bool WasReleased => Previous && !Current;

  }

  public void Update(in PlayerInput input) {
    Previous = Current;
    Current = input;
  }

}

}