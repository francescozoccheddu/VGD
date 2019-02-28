using System;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Networking;

namespace Wheeled.Core
{

    internal sealed class GameManager : MonoBehaviour
    {

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
            DontDestroyOnLoad(gameObject);
            s_instance = this;
        }

        private INetworkHost m_host;

        public void StartGameAsServer()
        {
            m_host.Stop();
            if (!(m_host is Server))
            {
                m_host = new Server();
            }

        }

        public void StartGameAsClient()
        {

        }

        public void DiscoveryServers()
        {

        }

        public void QuitGame()
        {
            m_host?.Stop();
        }

        public PlayerBehaviour InstantiatePlayerBehaviour()
        {
            throw new NotImplementedException();
        }

        private void OnDestroy()
        {
            m_host?.Stop();
            m_host = null;
            s_instance = null;
        }

    }

}
