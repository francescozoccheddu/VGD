﻿using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.PlayerView;
using Wheeled.Networking;

namespace Wheeled.Core
{
    public sealed partial class GameLauncher : MonoBehaviour
    {
        private static GameLauncher s_instance;

        private GameLauncher()
        {
        }

        public static GameLauncher Instance
        {
            get
            {
                if (s_instance == null)
                {
                    GameObject gameObject = new GameObject
                    {
                        name = "GameManager"
                    };
                    s_instance = gameObject.AddComponent<GameLauncher>();
                }
                return s_instance;
            }
        }

        public void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            NetworkManager.instance.Update();
        }

        private void OnDestroy()
        {
            NetworkManager.instance.Stop();
        }
    }
}