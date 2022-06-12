using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public static class ShapeMaster
    {
        static readonly int Id = 1200;
        static List<byte> playerIdList = new();
        static CustomOption ShapeMasterShapeshiftDuration;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.ShapeMaster);
            ShapeMasterShapeshiftDuration = CustomOption.Create(1210, Color.white, "ShapeMasterShapeshiftDuration", 10, 1, 1000, 1, Options.CustomRoleSpawnChances[CustomRoles.ShapeMaster]);
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
    }
}