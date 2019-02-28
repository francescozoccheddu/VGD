using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Gameplay;
using Wheeled.Networking;

namespace Wheeled.Core
{
    internal sealed partial class GameManager
    {

        public GameRoom? Room { get; private set; }

        public bool IsPlaying { get; private set; }

        private INetworkHost m_host;

        public void StartGameAsServer()
        {
            if (!IsPlaying)
            {
                m_host?.Stop();
                UnregisterGameRoomDiscoverEvent();
                if (!(m_host is Server))
                {
                    m_host = new Server();
                }
                IsPlaying = true;
                LoadScene(maps.gameScenes[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsServer has been ignored because a game is running");
            }
        }

        public void StartGameAsClient(GameRoom _room)
        {
            if (!IsPlaying)
            {
                if (!(m_host is Client))
                {
                    DestroyHost();
                    m_host = new Client();
                }
                Room = _room;
                IsPlaying = true;
                Client client = (Client) m_host;
                client.Start();
                LoadScene(maps.gameScenes[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsClient has been ignored because a game is running");
            }
        }

        public void DiscoveryServers()
        {
            if (!IsPlaying)
            {
                if (!(m_host is Client))
                {
                    DestroyHost();
                    m_host = new Client();
                }
                Client client = (Client) m_host;
                client.Start();
                client.StartServerDiscovery(9050);
                client.GameRoomDiscovered -= GameRoomDiscovered;
                client.GameRoomDiscovered += GameRoomDiscovered;
            }
            else
            {
                Debug.LogWarning("DiscoveryServers has been ignored because a game is running");
            }
        }

        public void QuitGame()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                Room = null;
                m_host?.Stop();
                SceneManager.LoadScene(maps.menuScene);
            }
            else
            {
                Debug.LogWarning("QuitGame has been ignored because no game is running");
            }
        }

        private void GameRoomDiscovered(GameRoom _room)
        {
            StartGameAsClient(_room);
        }

        private void UnregisterGameRoomDiscoverEvent()
        {
            Client client = m_host as Client;
            if (client != null)
            {
                client.GameRoomDiscovered -= GameRoomDiscovered;
            }
        }

        private void DestroyHost()
        {
            m_host?.Stop();
            UnregisterGameRoomDiscoverEvent();
            m_host = null;
        }

        private void LoadScene(int _scene)
        {
            SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single).completed += OnSceneLoaded;
        }

        private void OnSceneLoaded(AsyncOperation _operation)
        {
            if (m_host is Client)
            {
                ((Client) m_host).Connect((GameRoom) Room);
            }
            else if (m_host is Server)
            {
                ((Server) m_host).Start(9050);
            }
        }

        public PlayerBehaviour InstantiatePlayerBehaviour()
        {
            GameObject gameObject = Instantiate(pawns.playerPrefab);
            return gameObject.GetComponent<PlayerBehaviour>();
        }

    }
}
