using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class SettingsScreenBehaviour : MonoBehaviour
    {
        #region Public Fields

        public ListBehaviour colorList;
        public ListBehaviour headList;
        public InputField nameField;

        #endregion Public Fields

        #region Public Methods

        public void Save()
        {
            PlayerPreferences.ColorIndex = colorList.GetSelectedIndex();
            PlayerPreferences.HeadIndex = headList.GetSelectedIndex();
            PlayerPreferences.Name = nameField.text;
            PlayerPreferences.Save();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateScreen()
        {
            nameField.text = PlayerPreferences.Name ?? "";
            colorList.SetSelectedIndex(PlayerPreferences.ColorIndex);
            headList.SetSelectedIndex(PlayerPreferences.HeadIndex);
        }

        private void CreateTabs()
        {
            colorList.CreateChilds(Scripts.PlayerPreferences.colors.Length);
            headList.CreateChilds(Scripts.PlayerPreferences.heads.Length);
        }

        private void OnEnable()
        {
            CreateTabs();
            UpdateScreen();
        }

        #endregion Private Methods
    }
}