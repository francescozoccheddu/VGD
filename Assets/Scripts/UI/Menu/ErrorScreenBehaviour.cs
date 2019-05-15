using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class ErrorScreenBehaviour : MonoBehaviour
    {
        public Text messageLabel;

        public void SetMessage(string _message)
        {
            messageLabel.text = _message;
        }
    }
}