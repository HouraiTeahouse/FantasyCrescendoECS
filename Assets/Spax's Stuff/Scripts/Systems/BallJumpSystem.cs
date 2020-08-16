using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Jobs;

public class BallJumpSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle jobDeps)
    {
        Entities.ForEach((ref PhysicsVelocity physicsVelocity, in PhysicsMoveData moveData) =>
        {

            
        }).Run();

        return default;
    }
}
