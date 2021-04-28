﻿using HouraiTeahouse.FantasyCrescendo.Matches;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace HouraiTeahouse.FantasyCrescendo.Authoring {

[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class Player : MonoBehaviour, IConvertGameObjectToEntity {

  [System.NonSerialized] public byte PlayerID;

#pragma warning disable 0649
  [SerializeField] CharacterFrameData _frameData;
#pragma warning restore 0649

  PlayableGraph _graph;

  void OnDestroy() {
    if (_graph.IsValid()) {
      _graph.Destroy();
    }
  }
  
  public void Convert(Entity entity, EntityManager entityManager, 
                      GameObjectConversionSystem conversionSystem) {
    _graph = PlayableGraph.Create();
    _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

    var animator = GetComponentInChildren<Animator>();
    AnimationPlayableOutput.Create(_graph, name + " Animation", animator);

    entityManager.AddComponent(entity, typeof(PlayerConfig));
    entityManager.AddComponent(entity, typeof(PlayerComponent));
    entityManager.AddComponent(entity, typeof(PlayerInputComponent));
    entityManager.AddComponent(entity, typeof(CharacterFrame));
    entityManager.AddComponent(entity, typeof(CameraTarget));
    // Copy the player's position and transforms from the Entity to the GameObject
    entityManager.AddComponentData(entity, new PlayerCharacter {
      PlayableGraph = _graph,
      StateController = _frameData != null ? 
        _frameData.BuildController(new CharacterControllerBuildParams {
          Graph = _graph
        }) : 
        default(BlobAssetReference<CharacterStateController>)
    });

    // Copy the player's position and transforms from the Entity to the GameObject
    entityManager.AddComponent<CopyTransformToGameObject>(entity);
    entityManager.AddComponentObject(entity, transform);

    // Allocate player hitboxes for immediate player use.
    CreatePlayerHitboxes(entity, entityManager, CharacterFrame.kMaxPlayerHitboxCount);

    // Constrain players to only allow for X/Y movement and zero rotation.
    conversionSystem.World.GetOrCreateSystem<EndJointConversionSystem>().CreateJointEntity(
        this, new PhysicsConstrainedBodyPair(conversionSystem.GetPrimaryEntity(this), Entity.Null, false),
        CreateRigidbodyConstraints(Math.DecomposeRigidBodyTransform(transform.localToWorldMatrix))
    );

    // Copy bone positions and transforms from the GameObject to the Entities
    foreach (var child in GetComponentsInChildren<Transform>()) {
      if (child == transform) continue;
      Entity childEntity = conversionSystem.GetPrimaryEntity(child.gameObject);
      entityManager.AddComponent<CopyTransformFromGameObject>(childEntity);
      entityManager.AddComponentObject(childEntity, child);
    }
  }

  void CreatePlayerHitboxes(Entity player, EntityManager entityManager, int size) {
    var archetype = entityManager.CreateArchetype(
      typeof(Translation), typeof(Parent), typeof(Scale), 
      typeof(LocalToParent), typeof(LocalToWorld),
      typeof(Hitbox), typeof(HitboxState));

    var group = new NativeArray<LinkedEntityGroup>(size, Allocator.Temp);
    for (byte i = 0; i < size; i++) {
      var entity = entityManager.CreateEntity(archetype);
      entityManager.AddComponentData(entity, new Scale { Value = 1.0f });
      entityManager.AddComponentData(entity, new Hitbox {
        Radius = 1,
      });
      entityManager.AddComponentData(entity, new Parent { Value = player });
      entityManager.AddComponentData(entity, new HitboxState {
        Player = player,
        ID = i,
        PlayerID = this.PlayerID,
        Enabled = false
      });

      group[i] = new LinkedEntityGroup { Value = entity };

#if UNITY_EDITOR
      entityManager.SetName(entity, $"{name}, Hitbox {i + 1}");
#endif
    }

    entityManager.AddBuffer<LinkedEntityGroup>(player).AddRange(group);
  }

  PhysicsJoint CreateRigidbodyConstraints(RigidTransform offset) {
    var joint = new PhysicsJoint {
      BodyAFromJoint = BodyFrame.Identity,
      BodyBFromJoint = offset
    };
    var constraints = new FixedList128<Constraint>();
    constraints.Add(new Constraint {
      ConstrainedAxes = new bool3(false, false, true),
      Type = ConstraintType.Linear,
      Min = 0,
      Max = 0,
      SpringFrequency = Constraint.DefaultSpringFrequency,
      SpringDamping = Constraint.DefaultSpringDamping
    });
    constraints.Add(new Constraint {
      ConstrainedAxes = new bool3(true, true, true),
      Type = ConstraintType.Angular,
      Min = 0,
      Max = 0,
      SpringFrequency = Constraint.DefaultSpringFrequency,
      SpringDamping = Constraint.DefaultSpringDamping
    });
    joint.SetConstraints(constraints);
    return joint;
  }

}

}