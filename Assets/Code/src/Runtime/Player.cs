using HouraiTeahouse.FantasyCrescendo.Matches;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Authoring {

public class Player : MonoBehaviour, IConvertGameObjectToEntity {

  [NonSerialized] public uint PlayerID;

#pragma warning disable 0649
  [SerializeField] CharacterFrameData _frameData;
#pragma warning restore 0649
  
  public void Convert(Entity entity, EntityManager entityManager, 
                      GameObjectConversionSystem conversionSystem) {
    entityManager.AddComponent(entity, typeof(PlayerConfig));
    entityManager.AddComponent(entity, typeof(PlayerInputComponent));
    entityManager.AddComponent(entity, typeof(CharacterFrame));
    entityManager.AddComponentData(entity, new PlayerComponent {
      StateController = _frameData != null ? 
        _frameData.BuildController() : 
        default(BlobAssetReference<CharacterStateController>)
    });

    // Allocate player hitboxes for immediate player use.
    //CreatePlayerHitboxes(entity, entityManager, CharacterFrame.kMaxPlayerHitboxCount);
  }

  void CreatePlayerHitboxes(Entity player, EntityManager entityManager, int size) {
    var archetype = entityManager.CreateArchetype(
      typeof(Translation), typeof(Parent), typeof(Scale), 
      typeof(LocalToParent), typeof(LocalToWorld),
      typeof(Hitbox), typeof(HitboxState));

    var group = new NativeArray<LinkedEntityGroup>(size, Allocator.Temp);
    var hitboxes = new NativeArray<PlayerHitboxBuffer>(size, Allocator.Temp);
    for (var i = 0; i < size; i++) {
      var entity = entityManager.CreateEntity(archetype);
      entityManager.AddComponentData(entity, new Scale { Value = 1.0f });
      entityManager.AddComponentData(entity, new Parent { Value = player });
      entityManager.AddComponentData(entity, new HitboxState {
        PlayerID = this.PlayerID,
        Enabled = false
      });

      group[i] = new LinkedEntityGroup { Value = entity };
      hitboxes[i] = new PlayerHitboxBuffer { Hitbox = entity };

#if UNITY_EDITOR
      entityManager.SetName(entity, $"{name}, Hitbox {i + 1}");
#endif
    }

    entityManager.AddBuffer<LinkedEntityGroup>(player).AddRange(group);
    entityManager.AddBuffer<PlayerHitboxBuffer>(player).AddRange(hitboxes);
  }

}

}