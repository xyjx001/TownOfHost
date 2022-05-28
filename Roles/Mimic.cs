using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

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