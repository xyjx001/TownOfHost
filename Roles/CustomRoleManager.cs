using System;
using System.Linq;
using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public class CustomRoleManager
    {
        #region singleton
        public static CustomRoleManager Instance
        {
            get
            {
                if (_instance == null) Logger.Error("Instance Is Not Exists", "CustomRoleManager");
                return _instance;
            }
        }
        public static bool InstanceExists => _instance != null;
        public static bool TryGetInstance(out CustomRoleManager Instance)
        {
            Instance = _instance;
            return InstanceExists;
        }
        private CustomRoleManager() { }
        public static CustomRoleManager CreateInstance()
        {
            if (!InstanceExists) _instance = new CustomRoleManager();
            return _instance;
        }
        public static void RemoveInstance()
        {
            _instance = null;
        }
        private static CustomRoleManager _instance;
        #endregion
        public List<RoleBase> RoleInstances;
        public List<RolePlayer> RolePlayers;
        public RoleBase GetRoleInstance(CustomRoles roleId) => RoleInstances.Where(role => role.RoleId == roleId).FirstOrDefault();
        public RolePlayer GetRolePlayer(byte playerId) => RolePlayers.Where(rp => rp.player.PlayerId == playerId).FirstOrDefault();
        public void InitAllInstance()
        {
        }
        // RoleInstances/Players共通の呼び出し
        public void OnFixedUpdate()
        {
            RoleInstances.ForEach(role => role.OnFixedUpdate());
            RolePlayers.ForEach(rp => rp.OnFixedUpdate());
        }
        // RoleInstances内の関数呼び出し
        public void OnStartGame() => RoleInstances.ForEach(role => role.OnStartGame());
        public void OnStartMeeting() => RoleInstances.ForEach(role => role.OnStartMeeting());
        public void OnEndMeeting() => RoleInstances.ForEach(role => role.OnEndMeeting());
        public void HandleRpc(byte callId, MessageReader reader)
        {
            bool isHandled = false;
            foreach (var role in RoleInstances)
            {
                isHandled = role.HandleRpc(callId, reader);
                if (isHandled) break;
            }
            if (!isHandled)
            {
                Logger.Warn($"未処理のRPC: {(CustomRPC)callId}({callId})", "CustomRoleManager");
            }
        }

        // RolePlayers内の関数呼び出し
        public bool OnReportDeadBody(PlayerControl player, GameData.PlayerInfo target)
            => (GetRolePlayer(player.PlayerId)?.OnReportDeadBody(target)).GetValueOrDefault();
        public bool CanMurder(PlayerControl player, PlayerControl target)
            => (GetRolePlayer(player.PlayerId)?.CanMurder(target)).GetValueOrDefault();
        public bool OnMurdered(PlayerControl player, PlayerControl murderer)
            => (GetRolePlayer(player.PlayerId)?.OnMurdered(murderer)).GetValueOrDefault();
        public bool OnMurderPlayer(PlayerControl player, PlayerControl target)
            => (GetRolePlayer(player.PlayerId)?.OnMurderPlayer(target)).GetValueOrDefault();
    }
}