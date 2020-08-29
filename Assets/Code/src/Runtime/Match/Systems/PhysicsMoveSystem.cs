using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using HouraiTeahouse.FantasyCrescendo.Matches;
namespace HouraiTeahouse.FantasyCrescendo{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class PhysicsMoveSystem : SystemBase{
        protected override void OnUpdate() {
            float deltaTime = Time.fixedDeltaTime;

            Entities
            .WithoutBurst()
            .ForEach((ref PhysicsMoveData moveData, ref PhysicsVelocity vel, in GroundedTrigger grounded, in PlayerInputComponent input) =>
            {
                float3 newVel = vel.Linear;

                //float curInput = (Keyboard.current.aKey.isPressed) ? -1 : (Keyboard.current.dKey.isPressed) ? 1 : 0;
                float curInput = input.Current.Movement.x;

                //resetting the number of air jumps when the grouded trigger is triggered
                if (grounded.triggered){
                    moveData.currentAirJumps = 0;
                }
                //if the player wants to move
                if (curInput != 0){
                    //applies midair or grounded movement physcs dpeending on the grouded trigger's detection
                    if (grounded.triggered){
                        //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                        if (Mathf.Abs(newVel.x + curInput * moveData.groundedAcceleration) >= moveData.maxSpeed){
                            newVel.x = curInput * moveData.maxSpeed;
                        }
                        else{
                            newVel.x += curInput * moveData.groundedAcceleration;
                        }
                    }
                    else{
                        //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                        if (Mathf.Abs(newVel.x + curInput * moveData.airAcceleration) >= moveData.maxAirSpeed){
                            newVel.x = curInput * moveData.maxAirSpeed;
                        }
                        else{
                            newVel.x += curInput * moveData.airAcceleration;
                        }

                    }

                    //changes left or right facing
                    if (curInput >= 0){
                        moveData.facing = 1;
                    }
                    else{
                        moveData.facing = -1;
                    }

                    //if the player doesn't want to move
                }
                else{
                    if (grounded.triggered){
                        if (Mathf.Abs(newVel.x) - moveData.groundedFriction <= 0){
                            newVel.x = 0;
                        }
                        else{
                            //assigns the direction friction is applied by going in the opposite direction as the current velocity
                            int resistDir = (newVel.x < 0) ? 1 : -1;
                            newVel.x += resistDir * moveData.groundedFriction;
                        }
                    }
                    else{
                        if (Mathf.Abs(newVel.x) - moveData.airFriction <= 0){
                            newVel.x = 0;
                        }
                        else{
                            //assigns the direction friction is applied by going in the opposite direction as the current velocity
                            int resistDir = (newVel.x < 0) ? 1 : -1;
                            newVel.x += resistDir * moveData.airFriction;
                        }
                    }
                }

                if (newVel.y < 0 && input.Current.Movement.y < 0){
                    //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                    if (newVel.y + moveData.weight * -2 <= -moveData.maxFastFallSpeed){
                        newVel.y = -moveData.maxFastFallSpeed;
                    }
                    else{
                        newVel.y += moveData.weight * -2;
                    }
                }
                else{
                    //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                    if (newVel.y + moveData.weight * -1 <= -moveData.maxFallSpeed){
                        newVel.y = -moveData.maxFallSpeed;
                    }
                    else{
                        newVel.y += moveData.weight * -1;
                    }
                }

                if (grounded.triggered && input.WasPressed(HouraiTeahouse.FantasyCrescendo.Matches.PlayerInput.Button.JUMP)){
                    newVel.y = moveData.jumpForce;
                }
                else if (!grounded.triggered && input.WasPressed(HouraiTeahouse.FantasyCrescendo.Matches.PlayerInput.Button.JUMP) && moveData.currentAirJumps < moveData.maxAirJumps){
                    moveData.currentAirJumps += 1;
                    newVel.y = moveData.jumpForce;
                }

            vel.Linear = newVel;
        }).Run();
    }

}
}
