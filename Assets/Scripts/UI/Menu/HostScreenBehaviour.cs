using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;
using Wheeled.Core.Data;
using Wheeled.Networking;

namespace Wheeled.UI.Menu
{
    public sealed class HostScreenBehaviour : MonoBehaviour
    {
        public ListBehaviour arenaList;
        public InputField portField;

        private int m_arena;

        public void PrepareToStartGame()
        {
            m_arena = arenaList.Index;
        }

        public void StartGame()
        {
            int port = int.Parse(portField.text);
            if (PortValidatorBehaviour.IsInUseBySomeoneElse(port))
            {
                ScreenManagerBehaviour.SetError("Port is in use");
            }
            else
            {
                GameLauncher.Instance.StartGameAsServer(new GameRoomInfo
                {
                    endPoint = new IPEndPoint(IPAddress.Loopback, port),
                    arena = m_arena
                });
            }
        }

        private void OnEnable()
        {
            portField.text = "9060";
            arenaList.Count = Scripts.Scenes.arenas.Length;
            arenaList.Index = 0;
            portField.onValueChanged.Invoke(portField.text);
        }
    }
}