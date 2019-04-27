using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;
using Wheeled.Networking;

namespace Wheeled.Menu
{
    public sealed class HostScreenBehaviour : MonoBehaviour
    {
        #region Public Fields

        public ToggleGroup arenaGroup;
        public InputField portField;

        #endregion Public Fields

        #region Private Fields

        private int m_map = 0;

        #endregion Private Fields

        #region Public Methods

        public void StartGame()
        {
            int port = int.Parse(portField.text);
            int arena = arenaGroup.ActiveToggles().First().GetComponent<HostArenaEntryBehaviour>().arena;
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
            arenaGroup.SetAllTogglesOff();
            arenaGroup.transform.GetChild(m_map).GetComponent<Toggle>().isOn = true;
            arenaGroup.NotifyChildToggleValueChanged();
            portField.text = "9060";
            portField.NotifyValueChanged();
        }

        #endregion Private Methods
    }
}