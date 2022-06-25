using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public class Spy
    {
        #region static
        public static Spy Instance;
        public static readonly CustomRoles role = CustomRoles.Spy;
        static readonly int optionId = 21000;
        public static void SetupCustomOption()
        {
            //21000~21001
            Options.SetupSingleRoleOptions(optionId, role, 1);
            CustomOption parent = Options.CustomRoleSpawnChances[role];
            int i = 10; //以降、21010から1ずつ上がっていく。
            CustomOption.Create(optionId + i++, Color.white, "SpyCanVent", true, parent);
            CustomOption.Create(optionId + i++, Color.white, "ImpostorCanKillSpy", true, parent);
            CustomOption.Create(optionId + i++, Color.white, "SheriffCanKillSpy", false, parent);
        }
        #endregion

        public PlayerControl player;
        private Spy(PlayerControl player)
        {
            this.player = player;
        }
        public static Spy Create(PlayerControl player)
        {
            var role = new Spy(player);
            return role;
        }
        public void Init()
        {
        }
        public void FixedUpdate()
        {

        }
        public void OnStartMeeting()
        {

        }
    }
}