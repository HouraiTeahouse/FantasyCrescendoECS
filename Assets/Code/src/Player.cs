using System;
using Unity.Entities;
using UnityEngine;
using HouraiTeahouse.FantasyCrescendo.Matches;

namespace HouraiTeahouse.FantasyCrescendo.Authoring {

public class Player : MonoBehaviour, IConvertGameObjectToEntity {
  
  public void Convert(Entity entity, EntityManager entityManager, 
                      GameObjectConversionSystem conversionSystem) {
    entityManager.AddComponent(entity, typeof(PlayerComponent));
    entityManager.AddComponent(entity, typeof(PlayerConfig));
    entityManager.AddComponent(entity, typeof(PlayerInputComponent));
  }

}

}