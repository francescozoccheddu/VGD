using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Networking;

namespace Wheeled.Core
{

    public sealed partial class GameLauncher
    {

        public bool IsBusy => m_host?.IsStarted == true;
        public bool IsServer { get; private set; }
        private IGameHost m_host;

        public event GameRoomDiscoverEventHandler OnGameRoomDiscovered;

        private void UnregisterHostEvents()
        {
            if (m_host != null)
            {
                m_host.OnStopped -= GameStopped;
                if (m_host is Client client)
                {
                    client.OnConnected -= RoomJoined;
                    client.OnRoomDiscovered -= RoomDiscovered;
                }
            }
        }

        private Client EnsureClient()
        {
            if (m_host is Client)
            {
                return (Client) m_host;
            }
            else
            {
                DestroyHost();
                Client client = new Client();
                m_host = client;
                client.OnConnected += RoomJoined;
                client.OnRoomDiscovered += RoomDiscovered;
                client.OnStopped += GameStopped;
                return client;
            }
        }

        private Server EnsureServer()
        {
            if (m_host is Server)
            {
                return (Server) m_host;
            }
            else
            {
                DestroyHost();
                Server server = new Server();
                m_host = server;
                return server;
            }
        }

        public void StartGameAsServer(GameRoomInfo _room)
        {
            if (!IsBusy)
            {
                EnsureServer().Start(_room);
                LoadScene(ScriptManager.Scenes.game[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsServer has been ignored because a game is running or loading");
            }
        }

        public void StartGameAsClient(GameRoomInfo _room)
        {
            if (!IsBusy)
            {
                EnsureClient().Start(_room);
            }
            else
            {
                Debug.LogWarning("StartGameAsClient has been ignored because a game is running or loading");
            }
        }

        public void StartServerDiscovery(int _port)
        {
            if (!IsBusy)
            {
                EnsureClient().StartRoomDiscovery(_port);
            }
            else
            {
                Debug.LogWarning("DiscoveryServers has been ignored because a game is running or loading");
            }
        }

        public void QuitGame()
        {
            if (m_host != null)
            {
                m_host.Stop();
                LoadScene(ScriptManager.Scenes.menu);
            }
            else
            {
                Debug.LogWarning("QuitGame has been ignored because no game is running or a loading is in progress");
            }
        }

        private void LoadScene(int _scene)
        {
            SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single).completed += SceneLoaded;
        }

        private void DestroyHost()
        {
            UnregisterHostEvents();
            m_host?.Stop();
            m_host = null;
        }

        private void SceneLoaded(AsyncOperation _operation)
        {
            m_host?.GameReady();
        }

        private void RoomDiscovered(GameRoomInfo _room)
        {
            OnGameRoomDiscovered?.Invoke(_room);
        }

        private void RoomJoined(GameRoomInfo _room)
        {
            LoadScene(ScriptManager.Scenes.game[_room.map]);
        }

        private void GameStopped(GameHostStopCause _cause)
        {
            QuitGame();
        }

    }

}
