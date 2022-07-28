using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TownOfHost
{
    public class Speed : INotifyPropertyChanged
    {
        public static Speed speed = new();
        public event PropertyChangedEventHandler PropertyChanged;

        public const float SpeedMin = 0.0001f;
        public const float SpeedMax = 3f;
        private Dictionary<byte, float> _TmpAllPlayerSpeed = new();//一連の処理の流れで一時的に保持する為に使用
        public Dictionary<byte, float> TmpAllPlayerSpeed
        {
            get { return _TmpAllPlayerSpeed; }
            set
            {
                if (_TmpAllPlayerSpeed.SequenceEqual(value)) return;
                _TmpAllPlayerSpeed = value;
            }
        }
        private Dictionary<byte, float> _AllPlayerSpeed = new();
        public Dictionary<byte, float> AllPlayerSpeed
        {
            get { return _AllPlayerSpeed; }
            set
            {
                if (_AllPlayerSpeed.SequenceEqual(value)) return;
                _AllPlayerSpeed = value;
                _TmpAllPlayerSpeed = value;
                NotifyPropertyChanged("Speed");
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            Logger.Info("NotifyPropertyChanged", "Speed");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "Speed")
            {
                Logger.Info("Speed", "Speed");
                foreach (KeyValuePair<byte, float> playerSpeed in _AllPlayerSpeed)
                {
                    Logger.Info("Speedloop", "Speed");
                    PlayerControl player = Utils.GetPlayerById(playerSpeed.Key);

                    if (player != null) player.SetPlayerSpeed(playerSpeed.Value);

                }
                Logger.Info("SpeedloopEnd", "Speed");
            }
        }

    }

    // public class callSumple
    // {

    //     private void ButtonClick(object sender, EventArgs e)
    //     {
    //         Speed speed = new Speed();
    //         speed.AllPlayerSpeed

    //         Person.PropertyChanged += PersonPropertyChanged;
    //         Person.Name = "みかん";
    //     }

    //     Private void PersonPropertyChanged(object sender, PropertyChangedEventArgs e)
    //     {
    //         // 文字列でプロパティ名を判別
    //         if (e.PropertyName != "Name") return;

    //         var p = (PersonProperty)sender;
    //         MessageBox.Show(p.Name + "に変更されました");
    //     }
    //
    // }
}