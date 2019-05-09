using System.Linq;
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
            PlayerPreferences.ColorIndex = colorList.Index;
            PlayerPreferences.HeadIndex = headList.Index;
            PlayerPreferences.Name = nameField.text;
            PlayerPreferences.Save();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateScreen()
        {
            nameField.text = PlayerPreferences.Name ?? "";
            colorList.Index = PlayerPreferences.ColorIndex;
            headList.Index = PlayerPreferences.HeadIndex;
        }

        private void OnEnable()
        {
            colorList.Items = Scripts.PlayerPreferences.colors.Cast<object>().ToArray();
            headList.Items = Scripts.PlayerPreferences.heads;
        }

        #endregion Private Methods
    }
}