using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

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
        static Dictionary<byte, bool> isEvilGuesserExiled;
        static Dictionary<int, CustomRoles> RoleAndNumber;
        static bool IsEvilGuesser;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Guesser);
            EvilGuesserChance = CustomOption.Create(30110, Color.white, "EvilGuesserChance", 0, 0, 100, 10, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            ConfirmedEvilGuesser = CustomOption.Create(30120, Color.white, "ConfirmedEvilGuesser", 0, 0, 3, 1);
            Options.CustomRoleCounts.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
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
        }
        public static void Add(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            GuesserShootLimit[PlayerId] = GuesserCanKillCount.GetInt();
            isEvilGuesserExiled[PlayerId] = false;
            Logger.Info($"{Utils.GetPlayerById(PlayerId).name}={PlayerId}", "Guesser.Add");
            SetRoleAndNunber();
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
        public static void CheckForStartEvilGuesserMeeting(byte playerId)
        {
            if (isEvilGuesserExiled[playerId] == true) return;
            MeetingHud.Instance.RpcClose();
            isEvilGuesserExiled[playerId] = true;
            Utils.GetPlayerById(playerId).CmdReportDeadBody(null);
        }
        public static void GuesserShoot(PlayerControl killer, string subArgs1, string subArgs2)
        {
            Logger.Info("GuesserShoot開始", "guesser");
            if ((!killer.Is(CustomRoles.NiceGuesser) && killer.Is(CustomRoles.EvilGuesser)) || killer.Data.IsDead || !AmongUsClient.Instance.IsGameStarted) return;
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
                    Logger.Info($"{target.name}の役職取得{r}", "GuesserKill");
                    Logger.Info($"{target.name}の役職は{target.GetCustomRole()}でした。", "GuesserKill");
                    Logger.Info($"{GuesserShootLimit[killer.PlayerId]}=GuesserShootLimit[killer.PlayerId]", "GuesserKill");
                    if (target.GetCustomRole() == r)
                    {
                        Logger.Info("Guesserkill開始", "guesser");
                        if (target.GetCustomRole() == CustomRoles.Crewmate && !CanShootAsNomalCrewmate.GetBool()) return;
                        GuesserShootLimit[killer.PlayerId]--;
                        Logger.Info("GuesserShoot成功", "guesser");
                        killer.RpcMurderPlayer(target);
                        return;
                    }
                    if (target.GetCustomRole() != r)
                    {
                        killer.RpcMurderPlayer(killer);
                        return;
                    }
                }
            }
        }
        public static void SendShootChoices()
        {
            string text = "";
            Logger.Info("GuesserShootChoice検知", "guesser");
            if (RoleAndNumber.Count() == 0) return;
            for (var n = 1; n <= RoleAndNumber.Count(); n++)
            {
                Logger.Info($"{RoleAndNumber[n]}=RoleAndNumber[n],{n}=n", "guesser");
                text += string.Format("{0}:{1}\n", RoleAndNumber[n], n);
            }
            Logger.Info($"{text}=text", "guesser");
            Utils.SendMessage(text, byte.MaxValue);
        }
        public static void SetRoleAndNunber()
        {
            Logger.Info("setroleandnumber開始", "guesser");
            List<CustomRoles> roles = new();
            var i = 1;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!roles.Contains(pc.GetCustomRole())) roles.Add(pc.GetCustomRole());
            }
            Logger.Info($"{roles}=roles", "Guesser");
            foreach (var ro in roles)
            {
                RoleAndNumber.Add(i, ro);
                Logger.Info($"{i}=i,{ro}=role", "Guesser");
                i++;
            }
        }
    }
}