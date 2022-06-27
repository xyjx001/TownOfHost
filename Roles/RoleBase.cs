using System;
using System.Linq;
using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public abstract class RoleBase
    {
        #region singleton
        public static RoleBase Instance
        {
            get
            {
                if (_instance == null) Logger.Error("Instance Is Not Exists", "RoleBase");
                return _instance;
            }
        }
        public static bool InstanceExists => _instance != null;
        public static bool TryGetInstance(out RoleBase Instance)
        {
            Instance = _instance;
            return InstanceExists;
        }
        private RoleBase() { }
        public RoleBase GetOrCreateInstance()
        {
            if (InstanceExists) return _instance;
            CreateInstance();
            return InstanceExists ? _instance : throw new NotImplementedException("CreateInstanceメソッドが正常に実装されていません。_instanceがnullのままです。");
        }
        public abstract void CreateInstance();
        public static RoleBase _instance;
        #endregion

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