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
        public static readonly CustomRoles ThisRole = CustomRoles.Spy;
        static readonly int optionId = 21000;
        #region CustomOptions
        private static CustomOption opt_SpyCanVent;
        private static CustomOption opt_ImpostorCanKillSpy;
        private static CustomOption opt_SheriffCanKillSpy;
        #endregion
        #region OptionGetter
        public static bool IsRoleEnabled => ThisRole.IsEnable();
        public static float RoleChance => ThisRole.GetChance();
        public static int RoleCount => ThisRole.GetCount();
        public static bool SpyCanVent => opt_SpyCanVent.GetBool();
        public static bool ImpostorCanKillSpy => opt_ImpostorCanKillSpy.GetBool();
        public static bool SheriffCanKillSpy => opt_SheriffCanKillSpy.GetBool();
        public static bool ApplyDesyncForImpostors => IsRoleEnabled && ImpostorCanKillSpy;
        #endregion
        public static void SetupCustomOption()
        {
            //21000~21001
            Options.SetupSingleRoleOptions(optionId, ThisRole, 1);
            CustomOption parent = Options.CustomRoleSpawnChances[ThisRole];
            int i = 10; //以降、21010から1ずつ上がっていく。
            opt_SpyCanVent = CustomOption.Create(optionId + i++, Color.white, "SpyCanVent", true, parent);
            opt_ImpostorCanKillSpy = CustomOption.Create(optionId + i++, Color.white, "ImpostorCanKillSpy", true, parent);
            opt_SheriffCanKillSpy = CustomOption.Create(optionId + i++, Color.white, "SheriffCanKillSpy", false, parent);
        }
        #region 役職割り当て完了後の視点ごとの役職表
        /*↓seer
        | tgt> | Imp1 | Imp2 | Spy  |
        | Imp1 | Imp  | Sci  | Cr/E |
        | Imp2 | Sci  | Imp  | Cr/E |
        | Spy  | Imp  | Imp  | Cr/E |
        tgt = target, GA = 守護天使, Sci = 科学者, Cr/E = クルーまたはエンジニア
        */
        #endregion
        /* TODO-LIST
        - SheriffCanKillSpyのオプションを適用
        - ImpostorCanKillSpyのオプション名を修正
        - インポスターの特殊キルの対象で相方が除外されているものがある場合、Spyが透けるのでそれも変更する。
        */

        //戻り値: 元の処理を行うかどうか(trueで続行, falseで中断)
        public static bool Patch_RpcSetRoleReplacer_Release(PlayerControl target, RoleTypes roleType, CustomRpcSender sender, List<(PlayerControl, RoleTypes)> Storage)
        {
            if (!ApplyDesyncForImpostors) return true;
            if (roleType is RoleTypes.Impostor or RoleTypes.Shapeshifter)
            {
                foreach (var seer in PlayerControl.AllPlayerControls)
                {
                    if (seer == target || seer.PlayerId == 0) continue;
                    var seerPair = Storage.Where(pair => pair.Item1.PlayerId == seer.PlayerId).FirstOrDefault();
                    if (seerPair.Item2 is RoleTypes.Impostor or RoleTypes.Shapeshifter)
                    {
                        //相方限定RPC
                        sender.AutoStartRpc(target.NetId, (byte)RpcCalls.SetRole, seer.GetClientId())
                            .Write((ushort)RoleTypes.Scientist)
                            .EndRpc();
                    }
                }
                //一般用RPC
                sender.AutoStartRpc(target.NetId, (byte)RpcCalls.SetRole, -1)
                    .Write((ushort)roleType)
                    .EndRpc();
                target.SetRole(roleType);
                // -1のAutoStartRpcをした後のため、今開いているMessageのtargetは-1で確定
                return false;
            }
            return true;
        }
        //インポスター同士は視認し合える
        //スパイはインポスター視点のみ視認できる
        public static void SetNameColorData()
        {
            List<PlayerControl> looplist = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                CustomRoles role = pc.GetCustomRole();
                if (role.IsImpostor() || role == ThisRole) looplist.Add(pc);
            }

            foreach (var seer in looplist)
            {
                if (seer.Is(ThisRole)) continue;
                foreach (var target in looplist)
                {
                    NameColorManager.Instance.RpcAdd(seer.PlayerId, target.PlayerId, "#ff0000");
                }
            }
        }
        #endregion

        public PlayerControl player;
        private Spy(PlayerControl player)
        {
            this.player = player;
        }
        public static Spy Assign(PlayerControl player)
        {
            var role = new Spy(player);
            Main.AllPlayerCustomRoles[player.PlayerId] = Spy.ThisRole;
            return role;
        }
        public void FixedUpdate()
        {

        }
        public void OnStartMeeting()
        {

        }
    }
}