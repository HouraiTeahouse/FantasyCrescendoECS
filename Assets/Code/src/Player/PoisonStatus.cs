using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo {

public struct PoisonComponentData : IComponentData {
  public float DamagePerSecond;
}

public class PoisonSystem : SystemBase {
  protected override void OnUpdate() {
    float deltaTime = Time.fixedDeltaTime;
    Entities.ForEach((ref PlayerComponent player, in PoisonComponentData poison) => {
      player.Damage += deltaTime * poison.DamagePerSecond;
    }).Schedule();
  }
}

}