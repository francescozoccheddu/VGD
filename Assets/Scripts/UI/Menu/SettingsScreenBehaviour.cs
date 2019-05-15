using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class SettingsScreenBehaviour : MonoBehaviour
    {
        public ListBehaviour colorList;
        public ListBehaviour headList;
        public InputField nameField;

        public void Save()
        {
            PlayerPreferences.ColorIndex = colorList.Index;
            PlayerPreferences.HeadIndex = headList.Index;
            PlayerPreferences.Name = nameField.text;
            PlayerPreferences.Save();
        }

        private void UpdateScreen()
        {
            nameField.text = PlayerPreferences.Name ?? "";
            colorList.Index = PlayerPreferences.ColorIndex;
            headList.Index = PlayerPreferences.HeadIndex;
        }

        private void OnEnable()
        {
            colorList.Count = Scripts.PlayerPreferences.colors.Length;
            headList.Count = Scripts.PlayerPreferences.heads.Length;
            UpdateScreen();
        }
    }
}