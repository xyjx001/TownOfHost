using System.Collections.Generic;

namespace TownOfHost
{
    public static class Mimic
    {
        public static void Init()
        {
            MimicK.Init();
            MimicA.Init();
        }
    }
    public static class MimicK
    {
        static List<byte> playerIdList = new();
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
        public static void KillCheck(this PlayerControl killer, PlayerControl target)
        {
            Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
            killer.CustomSyncSettings(); //負荷軽減のため、killerだけがCustomSyncSettingsを実行
            killer.RpcGuardAndKill(target);
            if (!target.protectedByGuardian && !target.Is(CustomRoles.Bait))
                target.RpcExileV2();
            if (target.Is(CustomRoles.Bait))
                killer.RpcMurderPlayer(target);
            PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
            PlayerState.SetDead(target.PlayerId);
            killer.RpcShapeshift(target,false);
            if (target.Is(CustomRoles.Bait))
            {
                Logger.Info(target?.Data?.PlayerName + "はBaitだった", "MimicMurderPlayer");
                new LateTask(() => killer.CmdReportDeadBody(target.Data), 0.15f, "Bait Self Report");//ベイトのレポート処理
            }
            if (target.Is(CustomRoles.Terrorist))//CheckTerroristWinにタスク完了の条件があるためその条件は必要なし
            {
                Logger.Info(target?.Data?.PlayerName + "はTerroristだった", "MimicMurderPlayer");
                Utils.CheckTerroristWin(target.Data);
            }
            if (target.Is(CustomRoles.Trapper))
            {
                killer.TrapperKilled(target);
            }
            if (Main.ExecutionerTarget.ContainsValue(target.PlayerId))
            {
                target.ExecutionerTargetSetCustomRole();
            }
        }
    }
    public static class MimicA
    {
        static List<byte> playerIdList = new();
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
    }
}