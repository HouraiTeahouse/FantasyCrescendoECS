using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public class TestTriggerSystem : SystemBase{
        protected override void OnUpdate(){
            // for each teleporter with a trigger event buffer...
            Dependency = Entities.ForEach((ref GroundedTrigger groundedTrigger, in DynamicBuffer<TriggerEventBufferElement> triggerEventsBuffer) =>{
                // iterate on all trigger events that happened this frame...
                for (int i = 0; i < triggerEventsBuffer.Length; i++){

                    TriggerEventBufferElement triggerEvent = triggerEventsBuffer[i];

                    // If a character has entered the trigger, move its translation to the destination
                    if (triggerEvent.State == PhysicsEventState.Enter||triggerEvent.State == PhysicsEventState.Stay){
                        GroundedTrigger trigger = groundedTrigger;
                        trigger.triggered = true;
                        groundedTrigger = trigger;
                        //JobLogger.Log("triggered-B");
                    }
                    else if (triggerEvent.State == PhysicsEventState.Exit){
                        GroundedTrigger trigger = groundedTrigger;
                        trigger.triggered = false;
                        groundedTrigger = trigger;
                        //JobLogger.Log("triggered-B");
                    }
                }
            }).Schedule(Dependency);
        }

    }
}