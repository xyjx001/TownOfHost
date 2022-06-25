using System;
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
        public static void AssignRoleForRandomPlayer(ref List<PlayerControl> AllPlayers, CustomRpcSender sender)
        {
            System.Random rand = new();
            if (role.IsEnable())
            {
                for (int i = 0; i < role.GetCount(); i++)
                {
                    if (AllPlayers.Count <= 0) break;
                    var target = AllPlayers[rand.Next(0, AllPlayers.Count)];
                    AllPlayers.Remove(target);
                    Spy.Create(target);
                    Main.AllPlayerCustomRoles[target.PlayerId] = CustomRoles.Arsonist;
                    //Desync開始 そういえば最近ホストもDesyncするようになったんだってね。
                    if (target.PlayerId != 0)
                    {
                        int clientId = target.GetClientId();
                        sender.RpcSetRole(target, RoleTypes.Impostor, clientId);
                        //target視点: 他プレイヤー = 科学者
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == target) continue;
                            sender.RpcSetRole(pc, RoleTypes.Scientist, clientId);
                        }
                        //他プレイヤー: target = 科学者
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == target) continue;
                            if (pc.PlayerId == 0) target.SetRole(RoleTypes.Scientist); //ホスト視点用
                            else sender.RpcSetRole(target, RoleTypes.Scientist, pc.GetClientId());
                        }
                    }
                    else
                    {
                        //ホストは代わりに自視点守護天使, 他視点普通のクルーにする
                        target.SetRole(RoleTypes.GuardianAngel); //ホスト視点用
                        sender.RpcSetRole(target, RoleTypes.Crewmate);

                        //ただし、RoleBehaviourはGuardianAngelRole、RoleTypeはCrewmateという特殊な状態にする。
                        //これにより、RoleTypeがGuardianEngelになったら本当に守護天使化したと判別できる。
                        target.Data.Role.Role = RoleTypes.Crewmate;
                    }
                    target.Data.IsDead = true;
                }
            }
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