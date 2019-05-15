using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Player;

namespace Wheeled.HUD
{
    public sealed class ScoreBoardBehaviour : MonoBehaviour
    {
        
        public CanvasGroup antiScoreGroup;
        public GameObject scoreGroup;
        public Transform list;

        public GameObject entryPrefab;

        [Range(0.0f,1.0f)]
        public float antiScoreGroupAlpha;

        private static ScoreBoardBehaviour s_instance;

        public static void UpdateEntriesMain(IEnumerable<IReadOnlyPlayer> _players)
        {
            s_instance.UpdateEntries(_players);
        }

        public void UpdateEntries(IEnumerable<IReadOnlyPlayer> _players)
        {
            int i = 0;
            foreach (var player in _players)
            {
                GameObject gameObject;
                if (i >= list.childCount)
                {
                    gameObject = Instantiate(entryPrefab, list);
                }
                else
                {
                    gameObject = list.GetChild(i).gameObject;
                }
                gameObject.GetComponent<ScoreBoardEntryBehaviour>().Set(player);
                i++;
            }
            while (i < list.childCount)
            {
                Destroy(list.GetChild(i));
                i++;
            }
        }

        private void Awake()
        {
            s_instance = this;
        }

        private void Update()
        {
            bool isOpen = Input.GetButton("ScoreBoard");
            antiScoreGroup.alpha = isOpen ? antiScoreGroupAlpha : 1.0f;
            scoreGroup.SetActive(isOpen);
        }
        
    }
}