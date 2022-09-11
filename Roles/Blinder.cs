using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Blinder
    {
        private static readonly int Id = 21300;
        public static List<byte> playerIdList = new();
        public static List<byte> targetPlayerIdList = new();
        private static CustomOption OwnDecreaseVision;
        private static CustomOption TargetDecreaseVision;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Blinder);
            OwnDecreaseVision = CustomOption.Create(Id + 10, TabGroup.CrewmateRoles, Color.white, "BlinderOwnDecreaseVision", 0.5f, 0f, 10f, 0.5f, Options.CustomRoleSpawnChances[CustomRoles.Blinder]);
            TargetDecreaseVision = CustomOption.Create(Id + 11, TabGroup.CrewmateRoles, Color.white, "BlinderTargetDecreaseVision", 0.5f, 0f, 10f, 0.5f, Options.CustomRoleSpawnChances[CustomRoles.Blinder]);
        }
        public static void Init()
        {
            playerIdList = new();
            targetPlayerIdList = new();
        }
        public static void Add(PlayerControl blinder)
        {
            playerIdList.Add(blinder.PlayerId);
            //TODO:OwnDecreaseVision
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void OnMurderPlayer(PlayerControl killer, PlayerControl target)
        {
            if (target.Is(CustomRoles.Blinder))
                targetPlayerIdList.Add(killer.PlayerId);//キルした人をﾀｰｹﾞｯﾄにする
        }

        public static void OnCustomSyncSettings()
        {
            foreach (var target in targetPlayerIdList)
            {
                //TODO:キルした人の視野を下げる。
            }
        }
    }
}