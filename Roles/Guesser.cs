using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;
using System;
using InnerNet;
using HarmonyLib;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Guesser
    {
        static readonly int Id = 30100;
        static CustomOption EvilGuesserChance;
        static CustomOption ConfirmedEvilGuesser;
        static CustomOption CanShootAsNormalCrewmate;
        static CustomOption GuesserCanKillCount;
        static CustomOption CanKillMultipleTimes;
        static List<byte> playerIdList = new();
        static Dictionary<byte, int> GuesserShootLimit;
        public static Dictionary<byte, bool> isEvilGuesserExiled;
        static List<CustomRoles> ShootRoles;
        static LinkedList<(byte, string)> ChatMemory = new();
        public static Dictionary<byte, bool> IsSkillUsed;
        static bool IsEvilGuesser;
        public static bool IsEvilGuesserMeeting;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Guesser);
            EvilGuesserChance = CustomOption.Create(30110, TabGroup.NeutralRoles, Color.white, "EvilGuesserChance", 0, 0, 100, 10, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            ConfirmedEvilGuesser = CustomOption.Create(30120, TabGroup.NeutralRoles, Color.white, "ConfirmedEvilGuesser", 0, 0, 3, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            Options.CustomRoleCounts.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
            Options.CustomRoleSpawnChances.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
            CanShootAsNormalCrewmate = CustomOption.Create(30130, TabGroup.NeutralRoles, Color.white, "CanShootAsNormalCrewmate", true, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            GuesserCanKillCount = CustomOption.Create(30140, TabGroup.NeutralRoles, Color.white, "GuesserShootLimit", 1, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            CanKillMultipleTimes = CustomOption.Create(30150, TabGroup.NeutralRoles, Color.white, "CanKillMultipleTimes", false, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
        }
        public static bool SetGuesserTeam()//確定イビルゲッサーの人数とは別でイビルゲッサーかナイスゲッサーのどちらかに決める。
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
            ShootRoles = new();
            IsSkillUsed = new();
            ChatMemory = new();
            IsEvilGuesserMeeting = false;
        }
        public static void Add(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            GuesserShootLimit[PlayerId] = GuesserCanKillCount.GetInt();
            isEvilGuesserExiled[PlayerId] = false;
            IsSkillUsed[PlayerId] = false;
            IsEvilGuesserMeeting = false;
            for (int i = 0; i < 30; i++)
            {
                ChatMemory.AddLast((PlayerControl.LocalPlayer.PlayerId, "blank"));
            }
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void GuesserChatMemory(PlayerControl player, string text)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            ChatMemory.AddLast((player.PlayerId, text));
            ChatMemory.RemoveFirst();
        }
        public static void SendChat(PlayerControl killer)//Idea by AmongSUS
        {
            if (!AmongUsClient.Instance.AmHost) return;
            float delay = 0f;
            string Text = "";
            string text2 = "";
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            foreach (var ro in ShootRoles)
            {
                Text += string.Format("{0}:{1}\n", ro, (int)ro);
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                text2 += string.Format("{0}:{1}\n", pc.name, pc.PlayerId);
            }
            if (ChatMemory.Contains((player.PlayerId, Text))) ChatMemory.Remove((player.PlayerId, Text));
            if (ChatMemory.Contains((player.PlayerId, text2))) ChatMemory.Remove((player.PlayerId, text2));
            if (killer == PlayerControl.LocalPlayer) delay = 0.1f;
            new LateTask(() =>
            {
                foreach (var (pc, text) in ChatMemory)
                {
                    var player = Utils.GetPlayerById(pc);
                    if (player.Data.IsDead) player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, text);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SendChat, SendOption.None, -1);
                    writer.Write(text);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }, delay, "GuesserSendChat");
        }
        public static void SetRoleToGuesser(PlayerControl player)//ゲッサーをイビルとナイスに振り分ける
        {
            if (!player.Is(CustomRoles.Guesser)) return;
            if (IsEvilGuesser) Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.EvilGuesser;
            else Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.NiceGuesser;
        }
        public static void GuesserShoot(PlayerControl killer, string targetId, string targetrolenum)//ゲッサーが撃てるかどうかのチェック
        {
            if ((!killer.Is(CustomRoles.NiceGuesser) && !killer.Is(CustomRoles.EvilGuesser)) || killer.Data.IsDead || !AmongUsClient.Instance.IsGameStarted) return;
            //死んでるやつとゲッサーじゃないやつ、ゲームが始まってない場合は引き返す
            if (killer.Is(CustomRoles.NiceGuesser) && IsEvilGuesserMeeting) return;//イビルゲッサー会議の最中はナイスゲッサーは打つな
            if (!CanKillMultipleTimes.GetBool() && IsSkillUsed[killer.PlayerId] && !IsEvilGuesserMeeting) return;
            if (targetId == "show")
            {
                SendChat(killer);
                SendShootChoices(killer.PlayerId);
                return;
            }
            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (target.PlayerId == byte.Parse(targetId) && GuesserShootLimit[killer.PlayerId] != 0)//targetnameが人の名前で弾数が０じゃないなら続行
                {
                    SendChat(killer);
                    if ((int)target.GetCustomRole() == int.Parse(targetrolenum))//当たっていた場合
                    {
                        if ((target.GetCustomRole() == CustomRoles.Crewmate && !CanShootAsNormalCrewmate.GetBool()) || (target.GetCustomRole() == CustomRoles.Egoist && killer.Is(CustomRoles.EvilGuesser))) return;
                        //クルー打ちが許可されていない場合とイビルゲッサーがエゴイストを打とうとしている場合はここで帰る
                        GuesserShootLimit[killer.PlayerId]--;
                        IsSkillUsed[killer.PlayerId] = true;
                        PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
                        target.RpcGuesserMurderPlayer();//専用の殺し方
                        return;
                    }
                    if ((int)target.GetCustomRole() != int.Parse(targetrolenum))//外していた場合
                    {
                        PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Misfire);
                        killer.RpcGuesserMurderPlayer();
                        if (IsEvilGuesserMeeting)
                        {
                            IsEvilGuesserMeeting = false;
                            isEvilGuesserExiled[killer.PlayerId] = false;
                            MeetingHud.Instance.RpcClose();
                        }
                        return;
                    }
                }
            }
        }
        public static void AfterMeetingTasks()
        {
            foreach (var id in playerIdList)
            {
                IsSkillUsed[id] = false;
            }
        }
        public static void SendShootChoices(byte playerId)//番号と役職をチャットに表示
        {
            string text = "";
            string text2 = "";
            float delay = 0.04f;
            if (ShootRoles.Count() == 0) return;
            foreach (var ro in ShootRoles)
            {
                text += string.Format("{0}:{1}\n", ro, (int)ro);
            }
            if (Utils.GetPlayerById(playerId) == PlayerControl.LocalPlayer) delay = 0f;
            new LateTask(() => Utils.SendMessage(text, playerId), delay, "SendShootChoices");
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                text2 += string.Format("{0}:{1}\n", pc.name, pc.PlayerId);
            }
            if (Utils.GetPlayerById(playerId) == PlayerControl.LocalPlayer) delay = 0f;
            new LateTask(() => Utils.SendMessage(text2, playerId), delay, "SendShootChoices");
        }
        public static void RpcGuesserMurderPlayer(this PlayerControl pc)//ゲッサー用の殺し方
        {
            string text = "";
            new LateTask(() =>
            {
                MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, pc.GetClientId());
                MessageExtensions.WriteNetObject(MurderWriter, pc);
                AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);//ここまでの処理でターゲットで視点キルを発生させる
                pc.RpcExileV2();//それ以外のやつ視点で勝手に死んだことにする
                text += string.Format(GetString("KilledByGuesser"), pc.name);//ホスト以外死んだのがわからないのでチャットで送信
                Utils.SendMessage(text, byte.MaxValue);
            }, 0.2f, "Guesser Murder");


        }
        public static void SetRoleAndNumber()//役職を番号で管理
        {
            ShootRoles = new();
            foreach (var pc in PlayerControl.AllPlayerControls)//とりあえずアサインされた役職をすべて取りだす
            {
                if (!ShootRoles.Contains(pc.GetCustomRole())) ShootRoles.Add(pc.GetCustomRole());
            }
            if (Options.CanMakeMadmateCount.GetInt() != 0) ShootRoles.Add(CustomRoles.SKMadmate);//SKMadmateがいる際にはサイドキック前から候補に入れておく。
            if (CustomRoles.SchrodingerCat.IsEnable())//シュレネコがいる場合も役職変化前から候補に入れておく。
            {
                ShootRoles.Add(CustomRoles.MSchrodingerCat);
                if (Sheriff.IsEnable) ShootRoles.Add(CustomRoles.CSchrodingerCat);
                if (CustomRoles.Egoist.IsEnable()) ShootRoles.Add(CustomRoles.EgoSchrodingerCat);
                if (CustomRoles.Jackal.IsEnable()) ShootRoles.Add(CustomRoles.JSchrodingerCat);
            }
            ShootRoles = ShootRoles.OrderBy(a => Guid.NewGuid()).ToList();//会議画面で見たときに役職と順番が一緒で、役バレしたのでシャッフル
        }
        public static void OpenGuesserMeeting()
        {
            foreach (var gu in playerIdList)
            {
                if (isEvilGuesserExiled[gu])//ゲッサーの中から吊られた奴がいないかどうかの確認
                {
                    string text = "";
                    Utils.GetPlayerById(gu).CmdReportDeadBody(null);//会議を起こす
                    IsEvilGuesserMeeting = true;
                    text += GetString("EvilGuesserMeeting");
                    Utils.SendMessage(text, byte.MaxValue);
                }
            }
        }
    }
}