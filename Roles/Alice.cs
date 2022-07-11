using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public static class Alice
    {
        static readonly int Id = 50800;
        public static List<byte> playerIdList = new();
        public static CustomOption KillCooldown;
        public static CustomOption PlayerVision;
        public static CustomOption AffectedByLightsOut;

        public static HashSet<byte> CompleteWinCondition = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Alice);
            KillCooldown = CustomOption.Create(Id + 10, Color.white, "AliceKillCooldown", 30f, 0f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Alice]);
            PlayerVision = CustomOption.Create(Id + 11, Color.white, "AlicePlayerVision", 1.5f, 0.25f, 5f, 0.25f, Options.CustomRoleSpawnChances[CustomRoles.Alice]);
            AffectedByLightsOut = CustomOption.Create(Id + 12, Color.white, "AliceAffectedByLightsOut", false, Options.CustomRoleSpawnChances[CustomRoles.Alice]);
        }
        public static void Init()
        {
            playerIdList = new();
            CompleteWinCondition = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void SendRPC(byte id)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AliceList, SendOption.Reliable, -1);
            writer.Write(id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ReceiveRPC(MessageReader reader)
        {
            CompleteWinCondition.Add(reader.ReadByte());
        }
        public static bool CanWin(byte id) => CompleteWinCondition.Contains(id);
        public static void AddWinners(List<PlayerControl> winner)
        {
            foreach (var id in playerIdList)
            {
                var alice = Utils.GetPlayerById(id);
                if (alice == null) continue;
                if (CanWin(alice.PlayerId))
                {
                    Logger.Info("Runned", "Alice");
                    winner.Add(alice);
                    Main.additionalwinners.Add(AdditionalWinners.Alice);
                }
            }
        }
        public static void ApplyGameOptions(GameOptionsData opt)
        {
            opt.ImpostorLightMod = opt.CrewLightMod = PlayerVision.GetFloat();
            if (Utils.IsActive(SystemTypes.Electrical))
            {
                if (AffectedByLightsOut.GetBool())
                    opt.ImpostorLightMod /= 5;
                else
                    opt.CrewLightMod *= 5;
            }
        }
        public static void Killed(PlayerControl target)
        {
            if (!CompleteWinCondition.Contains(target.PlayerId))
            {
                Logger.Info(target.GetNameWithRole() + "をリストに追加", "Alice");
                CompleteWinCondition.Add(target.PlayerId); //インポスター陣営にキルされたアリスを追加
            }
        }
        public static void CheckAdditionalWin()
        {
            foreach (var alice in playerIdList)
            {
                if (CompleteWinCondition.Contains(alice)) continue;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.PlayerId == alice) continue;
                    if (pc.Is(RoleType.Neutral)) break; //第三陣営の場合は不要なので脱ループ
                    if (!PlayerState.isDead[alice])
                    {
                        Logger.Info(Utils.GetPlayerById(alice)?.GetNameWithRole() + "をリストに追加", "Alice");
                        CompleteWinCondition.Add(alice);
                        SendRPC(alice);
                        break;
                    }
                }
            }
        }
    }
}