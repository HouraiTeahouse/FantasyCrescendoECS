using HouraiTeahouse.FantasyCrescendo.Players;
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

  PlayerUIData[] _data;

  protected override void OnCreate() {
    _data = MatchConfig.CreatePlayerBuffer<PlayerUIData>();
  }

  protected override void OnUpdate() {
    var data = MatchConfig.CreateNativePlayerBuffer<PlayerUIData>(Allocator.TempJob);
    Entities
      //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
      .ForEach((in PlayerComponent player, in PlayerConfig config, 
                      in Translation translation) => {
      data[config.PlayerID] = new PlayerUIData {
        IsPresent = true,
        Config = config,
        PlayerData = player,
        WorldPosition = translation.Value
      };
    }).Schedule(this.Dependency).Complete();
    data.CopyTo(_data);
    data.Dispose();
  }

  public PlayerUIData GetPlayerUIData(int playerId) {
    Assert.IsTrue(playerId >= 0 && playerId < MatchConfig.kMaxSupportedPlayers);
    return _data[playerId];
  }

}

}