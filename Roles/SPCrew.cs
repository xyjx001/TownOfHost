using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hazel;

namespace TownOfHost
{
    public static class SPCrew
    {
        static readonly int Id = 23600;
        static List<byte> playerIdList = new();
        public static CustomOption RoleType;
        private static CustomOption OverrideScientistOptions;
        private static CustomOption ScientistCooldown;
        private static CustomOption ScientistBatteryCharge;
        private static CustomOption OverrideEngineerOptions;
        private static CustomOption EngineerCooldown;
        private static CustomOption EngineerInVentMaxTime;
        private static CustomOption OverrideDefaultOptions;
        private static CustomOption ButtonCount;
        private static CustomOption EmergencyCooldown;
        private static CustomOption IgnoreAnonymousVotes;
        private static CustomOption Vision;
        private static CustomOption HasImpostorVision;
        private static Options.OverrideTasksData SPCrewTasks;
        private static CustomOption AdvancedOptions;
        private static CustomOption CanReportDeadBody;
        private static CustomOption CanFixSabotages;
        private static CustomOption CanFixLightsOut;
        private static CustomOption CanFixComms;
        private static CustomOption CanFixO2;
        private static CustomOption CanFixReactor;

        private static readonly string[] RoleTypes =
        {
            CustomRoles.Crewmate.ToString(), CustomRoles.Scientist.ToString(), CustomRoles.Engineer.ToString()
        };
        private static readonly string[] FixSabotageOption =
        {
            "AllOn", "EachOption"
        };

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.SPCrew);
            RoleType = CustomOption.Create(Id + 10, Color.white, "SPRoleType", RoleTypes, RoleTypes[0], Options.CustomRoleSpawnChances[CustomRoles.SPCrew]);
            OverrideScientistOptions = CustomOption.Create(Id + 11, Color.white, "SPCOverrideScientistOptions", false, RoleType);
            ScientistCooldown = CustomOption.Create(Id + 12, Color.white, "SPCScientistCooldown", 15f, 0f, 180f, 2.5f, OverrideScientistOptions);
            ScientistBatteryCharge = CustomOption.Create(Id + 13, Color.white, "SPCScientistBatteryCharge", 5f, 0f, 255f, 2.5f, OverrideScientistOptions);
            OverrideEngineerOptions = CustomOption.Create(Id + 14, Color.white, "SPCOverrideEngineerOptions", false, RoleType);
            EngineerCooldown = CustomOption.Create(Id + 15, Color.white, "SPCEngineerCooldown", 30f, 0f, 180f, 2.5f, OverrideEngineerOptions);
            EngineerInVentMaxTime = CustomOption.Create(Id + 16, Color.white, "SPCEngineerInVentMaxTime", 15f, 0f, 180f, 2.5f, OverrideEngineerOptions);
            OverrideDefaultOptions = CustomOption.Create(Id + 17, Color.white, "SPOverrideDefaultOptions", false, Options.CustomRoleSpawnChances[CustomRoles.SPCrew]);
            ButtonCount = CustomOption.Create(Id + 18, Color.white, "SPButtonCount", 1, 0, 9, 1, OverrideDefaultOptions);
            EmergencyCooldown = CustomOption.Create(Id + 19, Color.white, "SPEmergencyCooldown", 20, 0, 60, 1, OverrideDefaultOptions);
            IgnoreAnonymousVotes = CustomOption.Create(Id + 20, Color.white, "SPIgnoreAnonymousVotes", false, OverrideDefaultOptions);
            Vision = CustomOption.Create(Id + 21, Color.white, "SPVision", 0.5f, 0f, 5f, 0.25f, OverrideDefaultOptions);
            HasImpostorVision = CustomOption.Create(Id + 22, Color.white, "SPHasImpostorVision", false, OverrideDefaultOptions);
            SPCrewTasks = Options.OverrideTasksData.Create(Id + 23, CustomRoles.SPCrew);
            AdvancedOptions = CustomOption.Create(Id + 24, Color.white, "SPAdvancedOptions", false, Options.CustomRoleSpawnChances[CustomRoles.SPCrew]);
            CanReportDeadBody = CustomOption.Create(Id + 25, Color.white, "SPCanReportDeadBody", true, AdvancedOptions);
            CanFixSabotages = CustomOption.Create(Id + 26, Color.white, "SPCanFixSabotages", FixSabotageOption, FixSabotageOption[0], AdvancedOptions);
            CanFixLightsOut = CustomOption.Create(Id + 27, Color.white, "SPCanFixLightsOut", true, CanFixSabotages);
            CanFixComms = CustomOption.Create(Id + 28, Color.white, "SPCanFixComms", true, CanFixSabotages);
            CanFixO2 = CustomOption.Create(Id + 29, Color.white, "SPCanFixO2", true, CanFixSabotages);
            CanFixReactor = CustomOption.Create(Id + 30, Color.white, "SPCanFixReactor", true, CanFixSabotages);
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
        public static void ApplyGameOptions(GameOptionsData opt, PlayerControl player)
        {
            if (RoleType.GetSelection() == 1 && OverrideScientistOptions.GetBool())
            {
                opt.RoleOptions.ScientistCooldown = ScientistCooldown.GetFloat();
                float BatteryCharge = ScientistBatteryCharge.GetFloat();
                if (BatteryCharge > 43f) BatteryCharge -= 256f;
                opt.RoleOptions.ScientistBatteryCharge = BatteryCharge;
            }
            if (RoleType.GetSelection() == 2 && OverrideEngineerOptions.GetBool())
            {
                opt.RoleOptions.EngineerCooldown = EngineerCooldown.GetFloat();
                opt.RoleOptions.EngineerInVentMaxTime = EngineerInVentMaxTime.GetFloat();
            }
            if (OverrideDefaultOptions.GetBool())
            {
                opt.EmergencyCooldown = EmergencyCooldown.GetInt();
                if (IgnoreAnonymousVotes.GetBool()) opt.AnonymousVotes = false;
                opt.CrewLightMod = Vision.GetFloat();
                opt.ImpostorLightMod = Vision.GetFloat();
                opt.SetVision(player, HasImpostorVision.GetBool());
            }
        }
        public static bool DisableReportDeadBody(PlayerControl player, GameData.PlayerInfo target) => target != null && player.Is(CustomRoles.SPCrew) && AdvancedOptions.GetBool() && !CanReportDeadBody.GetBool();
        public static bool DisableFixSabotages(PlayerControl player, SystemTypes systemType)
        {
            if (!player.Is(CustomRoles.SPCrew) || CanFixSabotages.GetSelection() == 0) return false;
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