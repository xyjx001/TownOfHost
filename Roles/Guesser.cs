using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;
using System;
using InnerNet;

namespace TownOfHost
{
    public static class Guesser
    {
        static readonly int Id = 30100;
        static CustomOption EvilGuesserChance;
        static CustomOption ConfirmedEvilGuesser;
        static CustomOption CanShootAsNomalCrewmate;
        static CustomOption GuesserCanKillCount;
        static List<byte> playerIdList = new();
        static Dictionary<byte, int> GuesserShootLimit;
        public static Dictionary<byte, bool> isEvilGuesserExiled;
        static Dictionary<int, CustomRoles> RoleAndNumber;
        static bool IsEvilGuesser;
        public static bool IsEvilGuesserMeeting;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Guesser);
            EvilGuesserChance = CustomOption.Create(30110, Color.white, "EvilGuesserChance", 0, 0, 100, 10, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            ConfirmedEvilGuesser = CustomOption.Create(30120, Color.white, "ConfirmedEvilGuesser", 0, 0, 3, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            Options.CustomRoleCounts.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
            Options.CustomRoleSpawnChances.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
            CanShootAsNomalCrewmate = CustomOption.Create(30130, Color.white, "CanShootAsNomalCrewmate", true, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            GuesserCanKillCount = CustomOption.Create(30140, Color.white, "GuesserShootLimit", 1, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
        }
        public static bool SetGuesserTeam()
        {
            float EvilGuesserRate = EvilGuesserChance.GetFloat();
            IsEvilGuesser = UnityEngine.Random.Range(1, 100) < EvilGuesserRate;
            return IsEvilGuesser;
        }
        public static void Init()
        {
            playerIdList = new();
            GuesserShootLimit = new();
            isEvilGuesserExiled = new();
            RoleAndNumber = new();
            IsEvilGuesserMeeting = false;
        }
        public static void Add(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            GuesserShootLimit[PlayerId] = GuesserCanKillCount.GetInt();
            isEvilGuesserExiled[PlayerId] = false;
            SetRoleAndNunber();
            IsEvilGuesserMeeting = false;
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void SetRoleToGuesser(PlayerControl player)
        {
            if (!player.Is(CustomRoles.Guesser)) return;
            if (IsEvilGuesser) Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.EvilGuesser;
            else Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.NiceGuesser;
        }
        public static void GuesserShoot(PlayerControl killer, string subArgs1, string subArgs2)
        {
            if ((!killer.Is(CustomRoles.NiceGuesser) && !killer.Is(CustomRoles.EvilGuesser)) || killer.Data.IsDead || !AmongUsClient.Instance.IsGameStarted) return;
            if (killer.Is(CustomRoles.NiceGuesser) && IsEvilGuesserMeeting) return;
            if (subArgs1 == "show")
            {
                SendShootChoices();
                return;
            }
            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (subArgs1 == $"{target.name}" && GuesserShootLimit[killer.PlayerId] != 0)
                {
                    RoleAndNumber.TryGetValue(int.Parse(subArgs2), out var r);
                    if (target.GetCustomRole() == r)
                    {
                        if ((target.GetCustomRole() == CustomRoles.Crewmate && !CanShootAsNomalCrewmate.GetBool()) || (target.GetCustomRole() == CustomRoles.Egoist && killer.Is(CustomRoles.EvilGuesser))) return;
                        GuesserShootLimit[killer.PlayerId]--;
                        target.RpcGuesserMurderPlayer(0f, true);
                        target.Data.IsDead = true;
                        return;
                    }
                    if (target.GetCustomRole() != r)
                    {
                        killer.RpcGuesserMurderPlayer(0f, false);
                        killer.Data.IsDead = true;
                        return;
                    }
                }
            }
        }
        public static void SendShootChoices()
        {
            string text = "";
            if (RoleAndNumber.Count() == 0) return;
            for (var n = 1; n <= RoleAndNumber.Count(); n++)
            {
                text += string.Format("{0}:{1}\n", RoleAndNumber[n], n);
            }
            Utils.SendMessage(text, byte.MaxValue);
        }
        public static void RpcGuesserMurderPlayer(this PlayerControl pc, float delay = 0f, bool sucsess = true)
        {
            string text = "";
            if ((Main.AliveImpostorCount == 1 && pc.GetCustomRole().IsImpostor()) || (Main.AliveImpostorCount == 1 && IsEvilGuesserMeeting)) pc.RpcMurderPlayer(pc);
            new LateTask(() =>
            {
                MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, pc.GetClientId());
                MessageExtensions.WriteNetObject(MurderWriter, pc);
                AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
            }, 0.2f + delay, "Guesser Murder");
            text += string.Format("{0}is killed by Guesser.", pc.name);
            Utils.SendMessage(text, byte.MaxValue);
            if (sucsess) Main.AfterMeetingDeathPlayers.TryAdd(pc.PlayerId, PlayerState.DeathReason.Kill);
            if (!sucsess) Main.AfterMeetingDeathPlayers.TryAdd(pc.PlayerId, PlayerState.DeathReason.Misfire);
        }
        public static void SetRoleAndNunber()
        {
            List<CustomRoles> roles = new();
            var i = 1;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!roles.Contains(pc.GetCustomRole())) roles.Add(pc.GetCustomRole());
            }
            roles = roles.OrderBy(a => Guid.NewGuid()).ToList();
            foreach (var ro in roles)
            {
                RoleAndNumber.Add(i, ro);
                i++;
            }
        }
        public static void OpenGuesserMeeting()
        {
            foreach (var gu in playerIdList)
            {
                if (isEvilGuesserExiled[gu])
                {
                    string text = "";
                    Utils.GetPlayerById(gu).CmdReportDeadBody(null);
                    IsEvilGuesserMeeting = true;
                    text += "It is time to shoot!";
                    Utils.SendMessage(text, byte.MaxValue);
                }
            }
        }
    }
}