using System.Collections.Generic;
using Hazel;

namespace TownOfHost
{
    public static class Alice
    {
        static readonly int Id = 50800;
        public static List<byte> playerIdList = new();

        public static HashSet<byte> CompleteWinCondition = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Alice);
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
        public static void Killed(PlayerControl target)
        {
            if (!CompleteWinCondition.Contains(target.PlayerId))
            {
                Logger.Info(target.Data.PlayerName + "をリストに追加", "Alice");
                CompleteWinCondition.Add(target.PlayerId); //インポスター陣営にキルされたアリスを追加
            }

        }
    }
}