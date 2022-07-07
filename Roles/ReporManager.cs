using System.Collections.Generic;
using HarmonyLib;

namespace TownOfHost
{
    public static class ReportManager
    {
        static readonly int Id = 3000;
        static List<byte> playerIdList = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.ReportManager);
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
            if (Main.ReportManagerKillInfo.ContainsKey(killer.PlayerId))
            {
                Main.ReportManagerKillInfo.Do(x =>
                {
                    if (x.Key == killer.PlayerId)
                    {
                        Main.IgnoreReportBody.Add(x.Value);
                        Logger.Info($"{Utils.GetPlayerById(x.Value).Data.PlayerName} : 通報禁止リストに追加", "ReportManager");
                    }
                });
                Main.ReportManagerKillInfo.Remove(killer.PlayerId);
            }
            Main.ReportManagerKillInfo.TryAdd(killer.PlayerId, target.PlayerId);
            Logger.Info($"{target.Data.PlayerName} : ReportManagerKillInfoに追加", "ReportManager");
        }
    }
}