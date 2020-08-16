using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct GroundedTrigger : IComponentData
{
    public void OnEnter()
    {
        Debug.Log("enter");
    }
    public void OnExit()
    {
        Debug.Log("exit");
    }
}
