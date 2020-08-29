using Unity.Entities;
namespace HouraiTeahouse.FantasyCrescendo{
    [GenerateAuthoringComponent]
    public struct PhysicsMoveData : IComponentData {
        public float groundedAcceleration;
        public float maxSpeed;
        public float airAcceleration;
        public float maxAirSpeed;
        public float maxFallSpeed;
        public float maxFastFallSpeed;
        public float weight;
        public float jumpForce;
        public int currentAirJumps;
        public int maxAirJumps;
        public float groundedFriction;
        public float airFriction;
        public int facing;
    }
}
