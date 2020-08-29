using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

public enum PhysicsEventState
{
    Enter,
    Stay,
    Exit,
}

[Serializable]
public struct TriggerEventBufferElement : IBufferElementData
{
    public Entity Entity;
    public PhysicsEventState State;

    public bool _isStale;
}

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class TriggerEventsSystem : SystemBase
{
    BuildPhysicsWorld _buildPhysicsWorldSystem;
    StepPhysicsWorld _stepPhysicsWorldSystem;
    EntityQuery _triggerEventsBufferEntityQuery;

    // todo; can maybe optimize by checking if chunk has changed?
    [BurstCompile]
    struct TriggerEventsPreProcessJob : IJobChunk
    {
        public ArchetypeChunkBufferType<TriggerEventBufferElement> TriggerEventBufferType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            BufferAccessor<TriggerEventBufferElement> triggerEventsBufferAccessor = chunk.GetBufferAccessor(TriggerEventBufferType);

            for (int i = 0; i < chunk.Count; i++)
            {
                DynamicBuffer<TriggerEventBufferElement> triggerEventsBuffer = triggerEventsBufferAccessor[i];

                for (int j = triggerEventsBuffer.Length - 1; j >= 0; j--)
                {
                    TriggerEventBufferElement triggerEventElement = triggerEventsBuffer[j];
                    triggerEventElement._isStale = true;
                    triggerEventsBuffer[j] = triggerEventElement;
                }
            }
        }
    }

    [BurstCompile]
    struct TriggerEventsJob : ITriggerEventsJob
    {
        public BufferFromEntity<TriggerEventBufferElement> TriggerEventBufferFromEntity;

        public void Execute(TriggerEvent triggerEvent)
        {
            ProcessForEntity(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            ProcessForEntity(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
        }

        private void ProcessForEntity(Entity entity, Entity otherEntity)
        {
            if(TriggerEventBufferFromEntity.Exists(entity))
            {
                DynamicBuffer<TriggerEventBufferElement> triggerEventBuffer = TriggerEventBufferFromEntity[entity];

                bool foundMatch = false;
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    TriggerEventBufferElement triggerEvent = triggerEventBuffer[i];

                    // If entity is already there, update to Stay
                    if (triggerEvent.Entity == otherEntity)
                    {
                        foundMatch = true;
                        triggerEvent.State = PhysicsEventState.Stay;
                        triggerEvent._isStale = false;
                        triggerEventBuffer[i] = triggerEvent;

                        break;
                    }
                }

                // If it's a new entity, add as Enter
                if(!foundMatch)
                {
                    triggerEventBuffer.Add(new TriggerEventBufferElement
                    {
                        Entity = otherEntity,
                        State = PhysicsEventState.Enter,
                        _isStale = false,
                    });
                }
            }
        }
    }

    [BurstCompile]
    struct TriggerEventsPostProcessJob : IJobChunk
    {
        public ArchetypeChunkBufferType<TriggerEventBufferElement> TriggerEventBufferType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            BufferAccessor<TriggerEventBufferElement> triggerEventsBufferAccessor = chunk.GetBufferAccessor(TriggerEventBufferType);

            for (int i = 0; i < chunk.Count; i++)
            {
                DynamicBuffer<TriggerEventBufferElement> triggerEventsBuffer = triggerEventsBufferAccessor[i];

                for (int j = triggerEventsBuffer.Length - 1; j >= 0; j--)
                {
                    TriggerEventBufferElement triggerEvent = triggerEventsBuffer[j];

                    if (triggerEvent._isStale)
                    {
                        if (triggerEvent.State == PhysicsEventState.Exit)
                        {
                            triggerEventsBuffer.RemoveAt(j);
                        }
                        else
                        {
                            triggerEvent.State = PhysicsEventState.Exit;
                            triggerEventsBuffer[j] = triggerEvent;
                        }
                    }
                }
            }
        }
    }

    protected override void OnCreate()
    {
        _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        _stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();

        EntityQueryDesc queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(PhysicsCollider),
                typeof(TriggerEventBufferElement),
            }
        };

        _triggerEventsBufferEntityQuery = GetEntityQuery(queryDesc);
    }

    protected override void OnUpdate()
    {
        Dependency = new TriggerEventsPreProcessJob
        {
            TriggerEventBufferType = GetArchetypeChunkBufferType<TriggerEventBufferElement>(),
        }.ScheduleParallel(_triggerEventsBufferEntityQuery, Dependency);

        Dependency = new TriggerEventsJob
        {
            TriggerEventBufferFromEntity = GetBufferFromEntity<TriggerEventBufferElement>(),
        }.Schedule(_stepPhysicsWorldSystem.Simulation, ref _buildPhysicsWorldSystem.PhysicsWorld, Dependency);

        Dependency = new TriggerEventsPostProcessJob
        {
            TriggerEventBufferType = GetArchetypeChunkBufferType<TriggerEventBufferElement>(),
        }.ScheduleParallel(_triggerEventsBufferEntityQuery, Dependency);
    }
}