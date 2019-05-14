using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;

namespace Wheeled.HUD
{
    public sealed class ScoreBoardEntryBehaviour : MonoBehaviour
    {

        public Text nameText;
        public Text killsText;
        public Text deathsText;
        public Text pingText;
        public RawImage iconImage;
        public RawImage background;

        internal void Set(IReadOnlyPlayer _player)
        {
            Color color = _player.GetColor();
            nameText.color = color;
            nameText.text = _player.GetName();
            killsText.color = color;
            killsText.text = _player.Kills.ToString();
            deathsText.color = color;
            deathsText.text = _player.Deaths.ToString();
            pingText.color = color;
            pingText.text = _player.Ping.ToString();
            iconImage.color = color;
            iconImage.texture = Scripts.PlayerPreferences.heads[_player.Info?.head ?? 0].icon;
            background.enabled = _player.IsLocal;
        }

    }
}