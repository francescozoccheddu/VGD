﻿using System;
using System.Net;
using UnityEngine;

namespace Wheeled.Core
{

    public struct GameRoomInfo
    {
        public IPEndPoint remoteEndPoint;
        public string name;
        public int map;
    }

    public sealed partial class GameManager : MonoBehaviour
    {

        public MapCollection maps;
        public PawnCollection pawns;

        private static GameManager s_instance;

        public static GameManager GetOrCreate()
        {
            if (s_instance == null)
            {
                GameObject gameObject = new GameObject
                {
                    name = "GameManager"
                };
                gameObject.AddComponent<GameManager>();
            }
            return s_instance;
        }

        public static GameManager Instance => GetOrCreate();

        private GameManager()
        {
            if (s_instance != null)
            {
                throw new NotSupportedException();
            }
            s_instance = this;
        }

        public void OnEnable()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            m_networkManager.Update();
        }

        private void OnDestroy()
        {
            m_networkManager.listener = null;
            m_networkManager.Stop();
            s_instance = null;
        }

    }

}
