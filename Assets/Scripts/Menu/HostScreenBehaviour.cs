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
        #region Public Fields

        public ListBehaviour arenaList;
        public InputField portField;

        #endregion Public Fields

        #region Public Methods

        public void StartGame()
        {
            int port = int.Parse(portField.text);
            int arena = arenaList.GetSelectedIndex();
            GameLauncher.Instance.StartGameAsServer(new GameRoomInfo
            {
                endPoint = new IPEndPoint(IPAddress.Loopback, port),
                map = (byte) arena
            });
        }

        #endregion Public Methods

        #region Private Methods

        private void OnEnable()
        {
            arenaList.CreateChilds(Scripts.Scenes.arenas.Length);
            arenaList.SetSelectedIndex(0);
            portField.text = "9060";
            portField.NotifyValueChanged();
        }

        #endregion Private Methods
    }
}