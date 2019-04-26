using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class SettingsScreenBehaviour : MonoBehaviour
    {
        #region Public Fields

        [Header("Widgets")]
        public ToggleGroup headGroup;
        public ToggleGroup colorGroup;
        public InputField nameField;

        [Header("Save Button")]
        public Button saveButton;
        public InteractableBehaviour interactable;

        [Header("Prefabs")]
        public GameObject colorTabPrefab;
        public GameObject headTabPrefab;

        #endregion Public Fields

        #region Public Methods

        public void TextChanged(string _text)
        {
            bool enabled = !string.IsNullOrEmpty(_text);
            SetSaveButtonEnabled(enabled);
        }

        public void Save()
        {
            PlayerPreferences.ColorIndex = colorGroup
                .ActiveToggles()
                .FirstOrDefault(_t => _t.isOn)
                ?.GetComponent<ColorPreferenceTabBehaviour>()
                .ColorIndex
                ?? 0;
            PlayerPreferences.HeadIndex = headGroup
                .ActiveToggles()
                .FirstOrDefault(_t => _t.isOn)
                ?.GetComponent<HeadPreferenceTabBehaviour>()
                .HeadIndex
                ?? 0;
            PlayerPreferences.Name = nameField.text;
            PlayerPreferences.Save();
            SetSaveButtonEnabled(false);
        }

        public void UpdateScreen()
        {
            {
                string name = PlayerPreferences.Name ?? "";
                nameField.text = name;
                SetSaveButtonEnabled(PlayerPreferences.IsValidName(name));
            }
            {
                colorGroup.allowSwitchOff = true;
                int color = PlayerPreferences.ColorIndex;
                foreach (Toggle tab in colorGroup.ActiveToggles())
                {
                    int index = tab.GetComponent<ColorPreferenceTabBehaviour>().ColorIndex;
                    tab.isOn = index == color;
                }
                colorGroup.allowSwitchOff = false;
            }
            {
                headGroup.allowSwitchOff = true;
                int head = PlayerPreferences.HeadIndex;
                foreach (Toggle tab in headGroup.ActiveToggles())
                {
                    int index = tab.GetComponent<HeadPreferenceTabBehaviour>().HeadIndex;
                    tab.isOn = index == head;
                }
                headGroup.allowSwitchOff = false;
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal void CreateTabs()
        {
            for (int i = 0; i < Scripts.PlayerPreferences.heads.Length; i++)
            {
                GameObject tab;
                if (i >= headGroup.transform.childCount)
                {
                    tab = Instantiate(headTabPrefab, headGroup.transform);
                }
                else
                {
                    tab = headGroup.transform.GetChild(i).gameObject;
                }
                tab.GetComponent<HeadPreferenceTabBehaviour>().HeadIndex = i;
                tab.GetComponent<Toggle>().group = headGroup;
            }
            for (int i = 0; i < Scripts.PlayerPreferences.colors.Length; i++)
            {
                GameObject tab;
                if (i >= colorGroup.transform.childCount)
                {
                    tab = Instantiate(colorTabPrefab, colorGroup.transform);
                }
                else
                {
                    tab = colorGroup.transform.GetChild(i).gameObject;
                }
                tab.GetComponent<ColorPreferenceTabBehaviour>().ColorIndex = i;
                tab.GetComponent<Toggle>().group = colorGroup;
            }
        }

        #endregion Internal Methods

        #region Private Methods

        private void SetSaveButtonEnabled(bool _enabled)
        {
            interactable.SetEnabled(_enabled);
            saveButton.interactable = _enabled;
        }

        private void OnEnable()
        {
            CreateTabs();
            UpdateScreen();
        }

        #endregion Private Methods
    }
}