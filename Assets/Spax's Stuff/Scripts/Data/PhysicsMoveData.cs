using Unity.Entities;

[GenerateAuthoringComponent]
public struct PhysicsMoveData : IComponentData
{
    public float moveSpeed;
    public float maxSpeed;
    public float maxFallSpeed;
    public float maxFastFallSpeed;
    public float weight;
    public float jumpForce;
    public float groundedFriction;
    public int facing;
}
