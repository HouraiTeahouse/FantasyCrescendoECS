using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

public class TestSystem : SystemBase {

  int frame;

  protected override void OnUpdate() {
    Debug.Log($"FRAME: {frame} {World.Time.DeltaTime}");
    frame++;
  }

}

}