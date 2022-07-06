using System.Collections.Generic;
using HarmonyLib;

namespace TownOfHost
{
    public static class TimeManager
    {
        static readonly int Id = 3000;
        static List<byte> playerIdList = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.TimeManager);
        }
        public static void Init()
        {
            playerIdList = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void OnPlayerKilled(PlayerControl killer, PlayerControl target)
        {
            if (Main.TimeManagerKillInfo.ContainsKey(killer.PlayerId))
            {
                Main.TimeManagerKillInfo.Do(x =>
                {
                    if (x.Key == killer.PlayerId)
                    {
                        Main.IgnoreReportBody.Add(x.Value);
                        Logger.Info($"{Utils.GetPlayerById(x.Value).Data.PlayerName} : 通報禁止リストに追加", "TimeManager");
                    }
                });
                Main.TimeManagerKillInfo.Remove(killer.PlayerId);
            }
            Main.TimeManagerKillInfo.TryAdd(killer.PlayerId, target.PlayerId);
            Logger.Info($"{target.Data.PlayerName} : TimeManagerKillInfoに追加", "TimeManager");
        }
    }
}