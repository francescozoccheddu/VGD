using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public sealed class ScoreBoardEntryBehaviour : MonoBehaviour
    {
        #region Public Fields

        public Text nameText;
        public Text killsText;
        public Text deathsText;
        public Text pingText;

        #endregion Public Fields

        #region Internal Methods

        internal void Set(string _name, int _kills, int _deaths, int _ping, Color _color)
        {
            nameText.text = _name ?? "Unknown";
            nameText.fontStyle = _name == null ? FontStyle.Italic : FontStyle.Normal;
            killsText.text = _kills.ToString();
            deathsText.text = _deaths.ToString();
            pingText.text = _ping.ToString();
            nameText.color = _color;
            killsText.color = _color;
            deathsText.color = _color;
            pingText.color = _color;
        }

        #endregion Internal Methods
    }
}