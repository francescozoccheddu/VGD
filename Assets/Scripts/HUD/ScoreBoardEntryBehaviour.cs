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
        public RawImage iconImage;

        #endregion Public Fields

        #region Internal Methods

        internal void Set(string _name, int _kills, int _deaths, int _ping, int _id, Texture2D _icon, Color _color)
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                nameText.text = string.Format("Player {0}", _id);
                nameText.fontStyle = FontStyle.Italic;
            }
            else
            {
                nameText.text = _name;
                nameText.fontStyle = FontStyle.Normal;
            }
            killsText.text = _kills.ToString();
            deathsText.text = _deaths.ToString();
            pingText.text = _ping.ToString();
            iconImage.texture = _icon;
            iconImage.color = _color;
            nameText.color = _color;
            killsText.color = _color;
            deathsText.color = _color;
            pingText.color = _color;
        }

        #endregion Internal Methods
    }
}