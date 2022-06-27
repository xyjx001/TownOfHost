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
        public static RoleBase _instance;
        #endregion
        #region OptionGetter
        public abstract bool IsRoleEnabled { get; }
        public abstract float RoleChance { get; }
        public abstract int RoleCount { get; }
        #endregion
        public CustomRoles RoleId { get; protected set; }
        public List<RolePlayer> Players;
        /// <summary>
        /// インスタンス生成時に実行されます。
        /// RoleIdの設定処理や、変数の初期化処理を入れてください。
        /// </summary>
        public abstract void CreateInstance();
        /// <summary>
        /// 役職割り当てが終わった時に実行されます。
        /// NameColorManagerの設定など、役職が決まらないとできない初期化処理を入れてください。
        /// </summary>
        public abstract void OnStartGame();
        /// <summary>
        /// FixedUpdate毎に実行されます。
        /// 常時実行していなければならない処理を入れてください。
        /// </summary>
        public abstract void OnFixedUpdate();
        /// <summary>
        /// 会議開始時に実行されます。
        /// 会議開始によるリセット処理などを入れてください。
        /// </summary>
        public abstract void OnStartMeeting();
        /// <summary>
        /// 会議終了時に実行されます。
        /// 会議終了によるリセット処理などを入れてください。
        /// </summary>
        public abstract void OnEndMeeting();
    }

    public abstract class RolePlayer
    {
        public RoleBase RoleInstance;
        public PlayerControl player;

        /// <summary>
        /// インスタンス生成時に実行されます。
        /// 変数の初期化処理などを入れてください。
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// FixedUpdate毎に実行されます。
        /// 常時実行していなければならない処理を入れてください。
        /// </summary>
        public abstract void OnFixedUpdate();
        /// <summary>
        /// プレイヤーが会議を開始しようとしたときに実行されます。
        /// 死体通報時の処理や、会議を開始できるかの判定処理などを入れてください。
        /// </summary>
        /// <param name="target">通報した死体のPlayerInfo ボタンの場合はnullになります。</param>
        /// <returns>false: 会議処理をキャンセルしない true: 会議をキャンセルする</returns>
        public abstract bool OnReportDeadBody(GameData.PlayerInfo target);
        /// <summary>
        /// プレイヤーがほかのプレイヤーをキルしようとしたときに実行されます。
        /// キルが可能かを判定し、キャンセル処理を入れてください。
        /// 実行順は"CanMurder"=>"OnMurdered"=>"OnMurderPlayer"です。
        /// </summary>
        /// <param name="target">キルしようとしたプレイヤー</param>
        /// <returns>false: キルをキャンセルしない true: キルをキャンセルする</returns>
        public abstract bool CanMurder(PlayerControl target);
        /// <summary>
        /// プレイヤーがほかのプレイヤーにキルされそうになったときに実行されます。
        /// キルのキャンセル処理や、キルされた時の処理を入れてください。
        /// 実行順は"CanMurder"=>"OnMurdered"=>"OnMurderPlayer"です。
        /// </summary>
        /// <param name="murderer">自分をキルしようとしたプレイヤー</param>
        /// <returns>false: キルをキャンセルしない true: キルをキャンセルする</returns>
        public abstract bool OnMurdered(PlayerControl murderer);
        /// <summary>
        /// プレイヤーがほかのプレイヤーをキルしようとしたときに実行されます。
        /// プレイヤーをキルした時の処理を書いてください。
        /// 実行順は"CanMurder"=>"OnMurdered"=>"OnMurderPlayer"です。
        /// </summary>
        /// <param name="target">キルしようとしたプレイヤー</param>
        /// <returns>false: キルをキャンセルしない true: キルをキャンセルする</returns>
        public abstract bool OnMurderPlayer(PlayerControl target);
    }
}