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
            portField.text = "9060";
            ipField.onValueChanged.Invoke("");
            portField.onValueChanged.Invoke("9060");
        }

        #endregion Private Methods
    }
}