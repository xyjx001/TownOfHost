using System.Collections.Generic;

namespace TownOfHost
{
    public static class Mimic
    {
        static readonly int Id = 2900;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Mimic);
        }
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
            Main.AllPlayerKillCooldown[killer.PlayerId] = Options.BHDefaultKillCooldown.GetFloat() * 2;
            killer.RpcGuardAndKill(target);
            target.RpcExileV2();
            killer.RpcShapeshift(target,false);
            killer.CustomSyncSettings();
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