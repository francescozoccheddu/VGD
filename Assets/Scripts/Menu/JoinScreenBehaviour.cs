using UnityEngine;
using UnityEngine.UI;

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