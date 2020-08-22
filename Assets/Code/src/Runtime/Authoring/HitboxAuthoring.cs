using UnityEngine;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Authoring {

public class HitboxAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

  public Hitbox Hitbox;
  public HitboxState State;

  public void Convert(Entity entity, EntityManager entityManager, 
                      GameObjectConversionSystem conversionSystem) {
    entityManager.AddComponentData(entity, Hitbox);
    entityManager.AddComponentData(entity, State);
  }

}

}
