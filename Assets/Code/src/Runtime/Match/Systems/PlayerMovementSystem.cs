using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class PlayerMovementSystem : SystemBase {

  protected override void OnUpdate() {
    var deltaTime = World.Time.DeltaTime;
   /* Entities.ForEach((ref Translation translation, in PlayerInputComponent input) => {
      translation.Value += (float3)input.Current.Movement * deltaTime * 5;
    }).Schedule();*/
  }

}

}
