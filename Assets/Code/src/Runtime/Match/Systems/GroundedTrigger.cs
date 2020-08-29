using Unity.Entities;
namespace HouraiTeahouse.FantasyCrescendo{
    [GenerateAuthoringComponent]
    public struct GroundedTrigger : IComponentData{
        public bool triggered;
    }
}
