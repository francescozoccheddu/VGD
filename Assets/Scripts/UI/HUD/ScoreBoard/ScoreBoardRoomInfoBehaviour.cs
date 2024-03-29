﻿using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.UI.HUD
{
    public class ScoreBoardRoomInfoBehaviour : MonoBehaviour
    {

        public Text text;

        private IPAddress GetLocalIPAddress()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return Dns.GetHostEntry(Dns.GetHostName())
                        .AddressList
                        .FirstOrDefault(_ip => _ip.AddressFamily == AddressFamily.InterNetwork);
            }
            return null;
        }

        private void OnEnable()
        {
            IPEndPoint endPoint = GameManager.Current.Room.endPoint;
            text.enabled = endPoint != null;
            if (endPoint != null)
            {
                IPAddress ip = GameManager.Current.Room.endPoint.Address;
                if (IPAddress.IsLoopback(ip))
                {
                    ip = GetLocalIPAddress() ?? ip;
                }
                text.text = string.Format("<color=\"#FFF7\">join at </color>{0}<color=\"#FFF7\">:</color>{1}", ip, endPoint.Port);
            }
        }

    }
}
