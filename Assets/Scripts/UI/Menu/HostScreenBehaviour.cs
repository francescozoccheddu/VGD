using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;
using Wheeled.Core.Data;
using Wheeled.Networking;

namespace Wheeled.Menu
{
    public sealed class HostScreenBehaviour : MonoBehaviour
    {
        public ListBehaviour arenaList;
        public InputField portField;

        public void StartGame()
        {
            int port = int.Parse(portField.text);
            int arena = arenaList.Index;
            GameLauncher.Instance.StartGameAsServer(new GameRoomInfo
            {
                endPoint = new IPEndPoint(IPAddress.Loopback, port),
                map = (byte) arena
            });
        }

        private void OnEnable()
        {
            portField.text = "9060";
            arenaList.Count = Scripts.Scenes.arenas.Length;
            arenaList.Index = 0;
        }
    }
}