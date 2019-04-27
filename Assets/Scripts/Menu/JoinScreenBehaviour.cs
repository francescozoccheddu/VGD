using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.Menu
{
    public sealed class JoinScreenBehaviour : MonoBehaviour
    {
        #region Public Fields

        public InputField ipField;
        public InputField portField;

        #endregion Public Fields

        #region Public Methods

        public void StartGame()
        {
            IPAddress ip = IPAddress.Parse(ipField.text);
            int port = int.Parse(portField.text);
            GameLauncher.Instance.StartGameAsClient(new IPEndPoint(ip, port));
        }

        #endregion Public Methods

        #region Private Methods

        private void OnEnable()
        {
            ipField.text = "";
            ipField.NotifyValueChanged();
            portField.text = "9060";
            portField.NotifyValueChanged();
        }

        #endregion Private Methods
    }
}