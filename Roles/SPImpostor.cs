using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public static class SPImpostor
    {
        static readonly int Id = 3500;
        static List<byte> playerIdList = new();
        public static CustomOption RoleType;
        private static CustomOption OverrideShapeShifterOptions;
        private static CustomOption ShapeshifterCooldown;
        private static CustomOption ShapeshifterDuration;
        private static CustomOption ShapeshifterLeaveSkin;
        private static CustomOption OverrideDefaultOptions;
        private static CustomOption ButtonCount;
        private static CustomOption EmergencyCooldown;
        private static CustomOption IgnoreAnonymousVotes;
        private static CustomOption Vision;
        private static CustomOption HasImpostorVision;
        private static CustomOption KillCooldown;
        private static CustomOption KillDistance;
        private static CustomOption AdvancedOptions;
        private static CustomOption CanSabotage;
        private static CustomOption CanUseVent;
        private static CustomOption CanReportDeadBody;
        private static CustomOption CanFixSabotages;
        private static CustomOption CanFixLightsOut;
        private static CustomOption CanFixComms;
        private static CustomOption CanFixO2;
        private static CustomOption CanFixReactor;

        private static readonly string[] RoleTypes =
        {
            CustomRoles.Impostor.ToString(), CustomRoles.Shapeshifter.ToString()
        };
        private static readonly string[] KillDistances =
        {
            "KillDistanceShort", "KillDistanceMedium", "KillDistanceLong"
        };
        private static readonly string[] FixSabotageOption =
        {
            "AllOn", "EachOption"
        };

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.SPImpostor);
            RoleType = CustomOption.Create(Id + 10, Color.white, "SPRoleType", RoleTypes, RoleTypes[0], Options.CustomRoleSpawnChances[CustomRoles.SPImpostor]);
            OverrideShapeShifterOptions = CustomOption.Create(Id + 11, Color.white, "SPIOverrideShapeShifterOptions", false, RoleType);
            ShapeshifterCooldown = CustomOption.Create(Id + 12, Color.white, "SPIShapeshifterCooldown", 10f, 5f, 180f, 2.5f, OverrideShapeShifterOptions);
            ShapeshifterDuration = CustomOption.Create(Id + 13, Color.white, "SPIShapeshifterDuration", 30f, 0f, 180f, 2.5f, OverrideShapeShifterOptions);
            ShapeshifterLeaveSkin = CustomOption.Create(Id + 14, Color.white, "SPIShapeshifterLeaveSkin", false, OverrideShapeShifterOptions);
            OverrideDefaultOptions = CustomOption.Create(Id + 15, Color.white, "SPOverrideDefaultOptions", false, Options.CustomRoleSpawnChances[CustomRoles.SPImpostor]);
            ButtonCount = CustomOption.Create(Id + 16, Color.white, "SPButtonCount", 1, 0, 9, 1, OverrideDefaultOptions);
            EmergencyCooldown = CustomOption.Create(Id + 17, Color.white, "SPEmergencyCooldown", 20, 0, 60, 1, OverrideDefaultOptions);
            IgnoreAnonymousVotes = CustomOption.Create(Id + 18, Color.white, "SPIgnoreAnonymousVotes", false, OverrideDefaultOptions);
            Vision = CustomOption.Create(Id + 19, Color.white, "SPVision", 1.5f, 0f, 5f, 0.25f, OverrideDefaultOptions);
            HasImpostorVision = CustomOption.Create(Id + 20, Color.white, "SPHasImpostorVision", true, OverrideDefaultOptions);
            KillCooldown = CustomOption.Create(Id + 21, Color.white, "SPIKillCooldown", 30, 0, 180, 1, OverrideDefaultOptions);
            KillDistance = CustomOption.Create(Id + 22, Color.white, "SPIKillDistance", KillDistances, KillDistances[1], OverrideDefaultOptions);
            AdvancedOptions = CustomOption.Create(Id + 23, Color.white, "SPAdvancedOptions", false, Options.CustomRoleSpawnChances[CustomRoles.SPImpostor]);
            CanSabotage = CustomOption.Create(Id + 24, Color.white, "SPICanSabotage", true, AdvancedOptions);
            CanUseVent = CustomOption.Create(Id + 25, Color.white, "SPICanUseVent", true, AdvancedOptions);
            CanReportDeadBody = CustomOption.Create(Id + 26, Color.white, "SPCanReportDeadBody", true, AdvancedOptions);
            CanFixSabotages = CustomOption.Create(Id + 27, Color.white, "SPCanFixSabotages", FixSabotageOption, FixSabotageOption[0], AdvancedOptions);
            CanFixLightsOut = CustomOption.Create(Id + 28, Color.white, "SPCanFixLightsOut", true, CanFixSabotages);
            CanFixComms = CustomOption.Create(Id + 29, Color.white, "SPCanFixComms", true, CanFixSabotages);
            CanFixO2 = CustomOption.Create(Id + 30, Color.white, "SPCanFixO2", true, CanFixSabotages);
            CanFixReactor = CustomOption.Create(Id + 31, Color.white, "SPCanFixReactor", true, CanFixSabotages);
        }
        public static void Init()
        {
            playerIdList = new();
        }
        public static void Add(PlayerControl pc)
        {
            playerIdList.Add(pc.PlayerId);
            if (OverrideDefaultOptions.GetBool())
                Main.AllPlayerNumEmergencyMeetings.Add(pc.PlayerId, ButtonCount.GetInt());
        }
        public static bool IsEnable() => playerIdList.Count > 0;
        public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
        public static void ApplyGameOptions(GameOptionsData opt, PlayerControl player)
        {
            if (RoleType.GetSelection() == 1 && OverrideShapeShifterOptions.GetBool())
            {
                opt.RoleOptions.ShapeshifterCooldown = ShapeshifterCooldown.GetFloat();
                opt.RoleOptions.ShapeshifterDuration = ShapeshifterDuration.GetFloat();
                opt.RoleOptions.ShapeshifterLeaveSkin = ShapeshifterLeaveSkin.GetBool();
            }
            if (OverrideDefaultOptions.GetBool())
            {
                opt.EmergencyCooldown = EmergencyCooldown.GetInt();
                if (IgnoreAnonymousVotes.GetBool()) opt.AnonymousVotes = false;
                opt.ImpostorLightMod = Vision.GetFloat();
                opt.CrewLightMod = Vision.GetFloat();
                opt.SetVision(player, HasImpostorVision.GetBool());
                Main.AllPlayerKillDistance[player.PlayerId] = KillDistance.GetSelection();
            }
        }
        public static bool DisableSabotage(PlayerControl player) => player.Is(CustomRoles.SPImpostor) && AdvancedOptions.GetBool() && !CanSabotage.GetBool();
        public static bool DisableVent(PlayerControl player) => player.Is(CustomRoles.SPImpostor) && AdvancedOptions.GetBool() && !CanUseVent.GetBool();
        public static bool DisableReportDeadBody(PlayerControl player, GameData.PlayerInfo target) => target != null && player.Is(CustomRoles.SPImpostor) && AdvancedOptions.GetBool() && !CanReportDeadBody.GetBool();
        public static bool DisableFixSabotages(PlayerControl player, SystemTypes systemType)
        {
            if (!player.Is(CustomRoles.SPImpostor) || CanFixSabotages.GetSelection() == 0) return false;
            switch (systemType)
            {
                case SystemTypes.Electrical:
                    return !CanFixLightsOut.GetBool();
                case SystemTypes.Comms:
                    return !CanFixComms.GetBool();
                case SystemTypes.LifeSupp:
                    return !CanFixO2.GetBool();
                case SystemTypes.Reactor:
                case SystemTypes.Laboratory:
                    return !CanFixReactor.GetBool();
                default:
                    return false;
            }
        }
    }
}