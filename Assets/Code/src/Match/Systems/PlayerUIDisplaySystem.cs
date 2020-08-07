using HouraiTeahouse.FantasyCrescendo.Players;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

public struct PlayerUIData {
  public PlayerConfig Config;
  public PlayerComponent PlayerData;
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class PlayerUIDisplaySystem : SystemBase {

  NativeArray<PlayerUIData> _playerIdData;

  protected override void OnCreate() {
    _playerIdData = MatchConfig.CreateNativePlayerBuffer<PlayerUIData>();
  }

  protected override void OnUpdate() {
    NativeArray<PlayerUIData> data = _playerIdData;
    Entities.ForEach((in PlayerComponent player, in PlayerConfig config) => {
      data[config.PlayerID] = new PlayerUIData {
        Config = config,
        PlayerData = player
      };
    }).Schedule();
  }

  protected override void OnDestroy() {
    _playerIdData.Dispose();
  }

  public PlayerUIData GetPlayerUIData(int playerId) {
    Assert.IsTrue(playerId >= 0 && playerId < MatchConfig.kMaxSupportedPlayers);
    return _playerIdData[playerId];
  }

}

}