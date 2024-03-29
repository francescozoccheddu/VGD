﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Action;

namespace Wheeled.UI.HUD
{
    public sealed class ScoreBoardBehaviour : MonoBehaviour
    {
        
        public CanvasGroup antiScoreGroup;
        public GameObject scoreGroup;
        public Transform list;

        public GameObject entryPrefab;

        public bool IsOpen { get; private set; }

        [Range(0.0f,1.0f)]
        public float antiScoreGroupAlpha;

        public void UpdateEntries()
        {
            var players = from p
                           in GameManager.Current.Players
                           where !p.IsQuit(GameManager.Current.Time)
                           select p;
            int i = 0;
            foreach (var player in players)
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
                Destroy(list.GetChild(i).gameObject);
                i++;
            }
        }

        private void Update()
        {
            IsOpen = Input.GetButton("ScoreBoard");
            antiScoreGroup.alpha = IsOpen ? antiScoreGroupAlpha : 1.0f;
            scoreGroup.SetActive(IsOpen);
            if (IsOpen)
            {
                UpdateEntries();
            }
        }
        
    }
}