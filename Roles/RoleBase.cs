using System;
using System.Linq;
using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public abstract class RoleBase
    {
        public static RoleBase Instance
        {
            get
            {
                if (_instance == null) Logger.Error("Instance Is Not Exists", "RoleBase");
                return _instance;
            }
        }
        public static RoleBase _instance;

        public List<RolePlayer> Players;
        public abstract void Init();
        public abstract void OnStartGame();
        public abstract void OnFixedUpdate();
    }

    public abstract class RolePlayer
    {
        public RoleBase RoleInstance;
        public PlayerControl player;

        public abstract void Init();
        public abstract void OnFixedUpdate();
        public abstract bool OnReportDeadBody();
    }
}