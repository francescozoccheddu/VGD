using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.UI.Menu
{
    public sealed class JoinScreenBehaviour : MonoBehaviour
    {
        public InputField ipField;
        public InputField portField;

        public void StartGame()
        {
            IPAddress ip = IPAddress.Parse(ipField.text);
            int port = int.Parse(portField.text);
            GameLauncher.Instance.StartGameAsClient(new IPEndPoint(ip, port));
        }

        private void OnEnable()
        {
            ipField.text = "";
            portField.text = "9060";
            ipField.onValueChanged.Invoke(ipField.text);
            portField.onValueChanged.Invoke(portField.text);
        }
    }
}