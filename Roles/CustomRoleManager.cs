using System;
using System.Linq;
using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public class CustomRoleManager
    {
        #region singleton
        public static CustomRoleManager Instance
        {
            get
            {
                if (_instance == null) Logger.Error("Instance Is Not Exists", "CustomRoleManager");
                return _instance;
            }
        }
        public static bool InstanceExists => _instance != null;
        public static bool TryGetInstance(out CustomRoleManager Instance)
        {
            Instance = _instance;
            return InstanceExists;
        }
        private CustomRoleManager() { }
        public CustomRoleManager CreateInstance()
        {
            if (!InstanceExists) _instance = new CustomRoleManager();
            return _instance;
        }
        public void RemoveInstance()
        {
            _instance = null;
        }
        public static CustomRoleManager _instance;
        #endregion
        public List<RoleBase> RoleInstances;
        public void InitAllInstance()
        {
        }
    }
}