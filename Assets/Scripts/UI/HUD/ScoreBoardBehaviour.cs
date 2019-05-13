using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Player;

namespace Wheeled.HUD
{
    public sealed class ScoreBoardBehaviour : MonoBehaviour
    {
        #region Public Properties

        public static bool IsOpen => s_instance.m_isOpen;

        #endregion Public Properties

        #region Public Fields

        public GameObject antiScoreGroup;
        public GameObject scoreGroup;
        public ScoreBoardEntryBehaviour[] entries;

        #endregion Public Fields

        #region Private Fields

        private static ScoreBoardBehaviour s_instance;
        private bool m_isOpen;

        #endregion Private Fields

        #region Internal Methods

        internal static void Update(IEnumerable<IReadOnlyPlayer> _players)
        {
            ScoreBoardEntryBehaviour[] entries = s_instance.entries;
            using (IEnumerator<IReadOnlyPlayer> enumerator = _players.OrderByDescending(_p => _p.Kills).ThenBy(_p => _p.Deaths).GetEnumerator())
            {
                bool hasNext;
                for (int i = 0; i < entries.Length; i++)
                {
                    hasNext = enumerator.MoveNext();
                    entries[i].gameObject.SetActive(hasNext);
                    if (hasNext)
                    {
                        IReadOnlyPlayer player = enumerator.Current;
                        int headIndex = player.Info?.head ?? 0;
                        int colorIndex = player.Info?.color ?? 0;
                        Texture2D icon = Scripts.PlayerPreferences.heads[headIndex].icon;
                        Color color = Scripts.PlayerPreferences.colors[colorIndex];
                        entries[i].Set(player.Info?.name, player.Kills, player.Deaths, player.Ping, player.Id, icon, color);
                    }
                }
            }
        }

        #endregion Internal Methods

        #region Private Methods

        private void Awake()
        {
            s_instance = this;
        }

        private void Update()
        {
            m_isOpen = Input.GetButton("ScoreBoard");
            antiScoreGroup.SetActive(!m_isOpen);
            scoreGroup.SetActive(m_isOpen);
        }

        #endregion Private Methods
    }
}