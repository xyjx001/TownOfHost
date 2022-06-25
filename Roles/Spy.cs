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
            Options.SetupSingleRoleOptions(optionId, role, 1);
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