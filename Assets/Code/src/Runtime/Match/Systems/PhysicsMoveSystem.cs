using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
public class PhysicsMoveSystem : JobComponentSystem
{

    protected override JobHandle OnUpdate(JobHandle jobDeps)
    {
        float deltaTime = Time.fixedDeltaTime;

        float curInput = (Keyboard.current.aKey.isPressed) ? -1 : (Keyboard.current.dKey.isPressed) ? 1 : 0;

        Entities.ForEach((ref PhysicsMoveData moveData, ref PhysicsVelocity vel) =>
        {
            float3 newVel = vel.Linear;

            //if the player wants to move
            if (curInput != 0)
            {
                //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                if (Mathf.Abs(newVel.x + curInput * moveData.moveSpeed) >= moveData.maxSpeed)
                {
                    newVel.x = curInput * moveData.maxSpeed;
                }
                else
                {
                    newVel.x += curInput * moveData.moveSpeed;
                }

                //changes left or right facing
                if (curInput >= 0)
                {
                    moveData.facing = 1;

                }
                else
                {
                    moveData.facing = -1;

                }
            }
            //if the player doesn't want to move
            else
            {
                if (Mathf.Abs(newVel.x) - moveData.groundedFriction <= 0)
                {
                    newVel.x = 0;
                }
                else
                {
                    //assigns the direction friction is applied by going in the opposite direction as the current velocity
                    int resistDir = (newVel.x < 0) ? 1 : -1;
                    newVel.x += resistDir * moveData.groundedFriction;
                }

            }

            if (newVel.y < 0 && Keyboard.current.sKey.isPressed)
            {
                //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                if (newVel.y + moveData.weight * -2 <= -moveData.maxFastFallSpeed)
                {
                    newVel.y = -moveData.maxFastFallSpeed;
                }
                else
                {
                    newVel.y += moveData.weight * -2;
                }
            }
            else
            {
                //velocity controller to make sure that the assigned velocity value isn't over the maximum velocity
                if (newVel.y + moveData.weight * -1 <= -moveData.maxFallSpeed)
                {
                    newVel.y = -moveData.maxFallSpeed;
                }
                else
                {
                    newVel.y += moveData.weight * -1;
                }
            }

            if (Keyboard.current.wKey.isPressed)
            {
                newVel.y = 10;
            }

            vel.Linear = newVel;

        }).Run();

        return default;
    }

    /*void SnapToGround(ref PlayerState state) {
        var pool = ArrayPool<RaycastHit>.Shared;
        var hits = pool.Rent(1);
        var offset = Vector3.up * CharacterController.height * 0.5f;
        var start = Vector3.up * PhysicsConfig.GroundedSnapOffset;
        var top = transform.TransformPoint(CharacterController.center + offset) + start;
        var bottom = transform.TransformPoint(CharacterController.center - offset) + start;
        var distance = PhysicsConfig.GroundedSnapOffset + PhysicsConfig.GroundedSnapDistance;
        var count = Physics.CapsuleCastNonAlloc(top, bottom, CharacterController.radius, Vector3.down, hits, 
                                                distance, PhysicsConfig.StageLayers, QueryTriggerInteraction.Ignore);
        if (count > 0) {
          CharacterController.Move(-Vector3.up * distance);
          state.Position = transform.position;
        }
        pool.Return(hits);
      }*/

    /*bool IsCharacterGrounded(in PlayerState state) {
      if (state.VelocityY > 0) return false;
      if (state.RespawnTimeRemaining > 0) return true;
      var center = Vector3.zero;
      var radius = 1f;
      if (CharacterController != null) {
        // TODO(james7132): Remove these magic numbers
        center = CharacterController.center - Vector3.up * (CharacterController.height * 0.50f - CharacterController.radius * 0.5f);
        radius = CharacterController.radius * 0.75f;
      }

      var stageLayers = Config.Get<PhysicsConfig>().StageLayers;
      center = transform.TransformPoint(center);

      var count = Physics.OverlapSphereNonAlloc(center, radius, colliderDummy, stageLayers, QueryTriggerInteraction.Ignore);
      return count != 0;
    }*/
}
