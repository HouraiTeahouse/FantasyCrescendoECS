using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Configs {

[CreateAssetMenu(menuName = "Config/Input Config")]
public class InputConfig : ScriptableObject {

  [Range(0, 128)] public byte DeadZone = (byte)39;
  [Range(0, 128)] public byte SmashThreshold = (byte)89;
  public uint SmashFrameWindow = 3;

}
    
}