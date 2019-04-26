using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class HostScreenBehaviour : MonoBehaviour
    {
        #region Public Fields

        public ToggleGroup arenaGroup;
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
            arenaGroup.SetAllTogglesOff();
            arenaGroup.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
            portField.text = "9060";
            portField.onValueChanged.Invoke("9060");
        }

        #endregion Private Methods
    }
}