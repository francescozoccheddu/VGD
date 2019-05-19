using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.UI.HUD
{
    public class ScoreBoardRoomInfoBehaviour : MonoBehaviour
    {

        public Text text;

        private void OnEnable()
        {
            IPEndPoint endPoint = GameManager.Current.Room.endPoint;
            text.text = string.Format("<color=\"#FFF7\">join at </color>{0}<color=\"#FFF7\">:</color>{1}", endPoint.Address.MapToIPv4(), endPoint.Port);
        }

    }
}
