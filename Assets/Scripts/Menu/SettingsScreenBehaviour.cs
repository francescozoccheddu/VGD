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

        [Header("Prefabs")]
        public GameObject colorTabPrefab;
        public GameObject headTabPrefab;

        #endregion Public Fields

        #region Public Methods

        public void Save()
        {
            PlayerPreferences.ColorIndex = colorGroup
                .ActiveToggles()
                .FirstOrDefault()
                ?.GetComponent<ColorPreferenceTabBehaviour>()
                .ColorIndex
                ?? 0;
            PlayerPreferences.HeadIndex = headGroup
                .ActiveToggles()
                .FirstOrDefault()
                ?.GetComponent<HeadPreferenceTabBehaviour>()
                .HeadIndex
                ?? 0;
            PlayerPreferences.Name = nameField.text;
            PlayerPreferences.Save();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateScreen()
        {
            {
                string name = PlayerPreferences.Name ?? "";
                nameField.text = name;
            }
            {
                colorGroup.SetAllTogglesOff();
                int color = PlayerPreferences.ColorIndex;
                colorGroup.transform.GetChild(color).GetComponent<Toggle>().isOn = true;
            }
            {
                headGroup.SetAllTogglesOff();
                int head = PlayerPreferences.HeadIndex;
                headGroup.transform.GetChild(head).GetComponent<Toggle>().isOn = true;
            }
        }

        private void CreateTabs()
        {
            for (int i = 0; i < Scripts.PlayerPreferences.heads.Length; i++)
            {
                GameObject tab;
                if (i >= headGroup.transform.childCount)
                {
                    tab = Instantiate(headTabPrefab, headGroup.transform);
                    tab.GetComponent<Toggle>().group = headGroup;
                }
                else
                {
                    tab = headGroup.transform.GetChild(i).gameObject;
                }
                tab.GetComponent<HeadPreferenceTabBehaviour>().HeadIndex = i;
            }
            for (int i = 0; i < Scripts.PlayerPreferences.colors.Length; i++)
            {
                GameObject tab;
                if (i >= colorGroup.transform.childCount)
                {
                    tab = Instantiate(colorTabPrefab, colorGroup.transform);
                    tab.GetComponent<Toggle>().group = colorGroup;
                }
                else
                {
                    tab = colorGroup.transform.GetChild(i).gameObject;
                }
                tab.GetComponent<ColorPreferenceTabBehaviour>().ColorIndex = i;
            }
        }

        private void OnEnable()
        {
            CreateTabs();
            UpdateScreen();
        }

        #endregion Private Methods
    }
}