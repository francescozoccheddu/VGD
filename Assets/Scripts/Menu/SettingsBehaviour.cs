using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class SettingsBehaviour : MonoBehaviour
    {
        #region Public Fields

        public ToggleGroup headGroup;
        public ToggleGroup colorGroup;
        public InputField nameField;
        public Button saveButton;
        public InteractableBehaviour interactable;

        #endregion Public Fields

        #region Public Methods

        public void TextChanged(string _text)
        {
            bool enabled = !string.IsNullOrEmpty(_text);
            SetEnabled(enabled);
        }

        public void Save()
        {
        }

        #endregion Public Methods

        #region Private Methods

        private void SetEnabled(bool _enabled)
        {
            interactable.SetEnabled(_enabled);
            saveButton.interactable = _enabled;
        }

        private void OnEnable()
        {
            SetEnabled(false);
        }

        #endregion Private Methods
    }
}