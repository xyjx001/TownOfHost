using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static TownOfHost.Options;

namespace TownOfHost
{
    public static class Deputy
    {
        private static readonly int Id = 21200;
        public static List<byte> playerIdList = new();

        private static readonly string[] SettingSelection =
        {
            "SheriffKillCooldown", "SheriffShotLimit"
        };
        private static CustomOption ChangeOption;
        private static CustomOption DecreaseKillCooldown;
        private static CustomOption IncreaseShotLimit;
        private static CustomOption ChangeNumOfTasks;

        private static Dictionary<byte, byte> ParentSheriff = new();

        private static readonly List<PlayerControl> SheriffList = new();
        private static Dictionary<byte, int> CountTasksBeforeAbility = new();
        public static void SetupCustomOption()
        {
            var spawnOption = CustomOption.Create(Id, Utils.GetRoleColor(CustomRoles.Deputy), CustomRoles.Deputy.ToString(), rates, rates[0], CustomRoleSpawnChances[CustomRoles.Sheriff])
                .HiddenOnDisplay(true)
                .SetGameMode(CustomGameMode.Standard);
            CustomRoleSpawnChances.Add(CustomRoles.Deputy, spawnOption);
            CustomRoleCounts.Add(CustomRoles.Deputy, CustomRoleCounts.GetValueOrDefault(CustomRoles.Sheriff));

            ChangeOption = CustomOption.Create(Id + 10, Color.white, "DeputyChangeOption", SettingSelection, SettingSelection[0], CustomRoleSpawnChances[CustomRoles.Deputy]);
            DecreaseKillCooldown = CustomOption.Create(Id + 11, Color.white, "DeputyDecreaseKillCooldown", 2f, 1f, 5f, 1f, CustomRoleSpawnChances[CustomRoles.Deputy]);
            IncreaseShotLimit = CustomOption.Create(Id + 12, Color.white, "DeputyIncreaseShotLimit", 1f, 1f, 2f, 1f, CustomRoleSpawnChances[CustomRoles.Deputy]);
            ChangeNumOfTasks = CustomOption.Create(Id + 13, Color.white, "DeputyChangeNumOfTasks", 3f, 1f, 10f, 1f, CustomRoleSpawnChances[CustomRoles.Deputy]);
        }
        public static void Init()
        {
            playerIdList = new();
            ParentSheriff = new();
            CountTasksBeforeAbility = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
                SheriffList.Add(pc);
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            CountTasksBeforeAbility.Add(playerId, 0);

            SheriffList.RemoveAll(x => !x.Is(CustomRoles.Sheriff));
            var rand = new System.Random();

            var parent = SheriffList[rand.Next(0, SheriffList.Count)];
            SheriffList.Remove(parent);
            ParentSheriff.Add(playerId, parent.PlayerId);
            Logger.Info(Utils.GetPlayerById(playerId)?.Data.PlayerName + " => " + parent.Data.PlayerName, "Deputy");
            SendRPC(playerId, parent.PlayerId);
        }
        public static bool IsEnable => playerIdList.Count > 0;
        private static void SendRPC(byte deputy, byte parent)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncParentSheriff, SendOption.Reliable);
            writer.Write(deputy);
            writer.Write(parent);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ReceiveRPC(MessageReader reader)
        {
            byte deputy = reader.ReadByte();
            byte parent = reader.ReadByte();
            ParentSheriff[deputy] = parent;
        }
        public static string VisibleParent(PlayerControl seer, PlayerControl target, string targetName)
        {
            var Condition = seer.Is(CustomRoles.Deputy) && target.Is(CustomRoles.Sheriff) && target.PlayerId == ParentSheriff.GetValueOrDefault(seer.PlayerId);
            return Condition ? Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Sheriff), targetName) : targetName;
        }
        public static void OnCompleteTask(PlayerControl player)
        {
            if (!player.Is(CustomRoles.Deputy)) return; //デピュティ以外処理しない

            CountTasksBeforeAbility[player.PlayerId]++;
            if (CountTasksBeforeAbility[player.PlayerId] == ChangeNumOfTasks.GetInt())
            {
                if (ParentSheriff.TryGetValue(player.PlayerId, out var parentId))
                {
                    var parent = Utils.GetPlayerById(parentId);
                    if (ChangeOption.GetSelection() == 0) //キルクールを減らすなら
                    {
                        Sheriff.CurrentKillCooldown[parentId] -= DecreaseKillCooldown.GetFloat();
                        parent?.CustomSyncSettings();
                    }
                    else
                    {
                        Sheriff.ShotLimit[parentId] += IncreaseShotLimit.GetFloat();
                        Utils.NotifyRoles();
                    }
                }
                CountTasksBeforeAbility[parentId] = 0;
            }
        }
    }
}