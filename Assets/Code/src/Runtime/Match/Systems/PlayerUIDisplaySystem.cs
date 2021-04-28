using UnityEngine;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct PlayerUIData {
  public bool IsPresent;
  public PlayerConfig Config;
  public PlayerComponent PlayerData;
  public Vector3 WorldPosition;
}

[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class PlayerUIDisplaySystem : SystemBase {

  NativeArray<PlayerUIData> _data;

  protected override void OnCreate() {
    _data = MatchConfig.CreateNativePlayerBuffer<PlayerUIData>();
  }

  protected override void OnUpdate() {
    NativeArray<PlayerUIData> data = _data;
    Entities
      .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
      .ForEach((in PlayerComponent player, in PlayerConfig config, 
                      in Translation translation) => {
      data[config.PlayerID] = new PlayerUIData {
        IsPresent = true,
        Config = config,
        PlayerData = player,
        WorldPosition = translation.Value
      };
    }).Schedule();
  }

  protected override void OnDestroy() {
    _data.Dispose();
  }

  public PlayerUIData GetPlayerUIData(int playerId) {
    CompleteDependency();
    Assert.IsTrue(playerId >= 0 && playerId < MatchConfig.kMaxSupportedPlayers);
    return _data[playerId];
  }

}

}