using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public static class ShapeMaster
    {
        static readonly int Id = 1200;
        static List<byte> playerIdList = new();
        public static CustomOption ShapeMasterShapeshiftDuration;
        static CustomOption DisableShapeMasterShapeshiftAnimation;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.ShapeMaster);
            ShapeMasterShapeshiftDuration = CustomOption.Create(1210, Color.white, "ShapeMasterShapeshiftDuration", 10, 1, 1000, 1, Options.CustomRoleSpawnChances[CustomRoles.ShapeMaster]);
            DisableShapeMasterShapeshiftAnimation = CustomOption.Create(1210, Color.white, "ShapeMasterShapeshiftDuration", false, Options.CustomRoleSpawnChances[CustomRoles.ShapeMaster]);
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
        public static void ShapeShiftCheck(this PlayerControl pc, PlayerControl target, bool shapeshifting)
        {
            if (pc == null || pc.Data.IsDead) return;
            if (shapeshifting)
            {
                if (DisableShapeMasterShapeshiftAnimation.GetBool())
                    pc.RpcShapeshiftV2(target,false);
                else
                    pc.RpcShapeshiftV2(target,true);
            }
            else
            {
                if (DisableShapeMasterShapeshiftAnimation.GetBool())
                    pc.RpcRevertShapeshift(false);
                else
                    pc.RpcRevertShapeshift(true);
            }
        }
    }
}