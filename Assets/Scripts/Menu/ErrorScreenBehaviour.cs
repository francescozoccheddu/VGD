using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class ErrorScreenBehaviour : MonoBehaviour
    {
        #region Public Fields

        public Text messageLabel;

        #endregion Public Fields

        #region Public Methods

        public void SetMessage(string _message)
        {
            messageLabel.text = _message;
        }

        #endregion Public Methods
    }
}