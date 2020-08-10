using HouraiTeahouse.FantasyCrescendo.Matches;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Authoring {

public class Player : MonoBehaviour, IConvertGameObjectToEntity {

#pragma warning disable 0649
  [SerializeField] CharacterFrameData _frameData;
#pragma warning restore 0649
  
  public void Convert(Entity entity, EntityManager entityManager, 
                      GameObjectConversionSystem conversionSystem) {
    entityManager.AddComponent(entity, typeof(PlayerConfig));
    entityManager.AddComponent(entity, typeof(PlayerInputComponent));
    entityManager.AddComponentData(entity, new PlayerComponent {
      StateController = _frameData != null ? 
        _frameData.BuildController() : 
        default(BlobAssetReference<CharacterStateController>)
    });

    // Allocate 64 player hitboxes for immediate player use.
    var hitboxArray = CreatePlayerHitboxes(entityManager, CharacterFrame.kMaxPlayerHitboxCount);
    entityManager.AddBuffer<PlayerHitboxBuffer>(entity).AddRange(hitboxArray);
  }


  NativeArray<PlayerHitboxBuffer> CreatePlayerHitboxes(EntityManager entityManager, int size) {
    var hitboxArchetype = entityManager.CreateArchetype(
      typeof(Translation), typeof(Hitbox), typeof(Disabled));
    var hitboxArray = new NativeArray<PlayerHitboxBuffer>(size, Allocator.Temp);
    for (var i = 0; i < size; i++) {
      hitboxArray[i] = new PlayerHitboxBuffer { 
        Hitbox = entityManager.CreateEntity(hitboxArchetype) 
      };
    }
    return hitboxArray;
  }

}

}