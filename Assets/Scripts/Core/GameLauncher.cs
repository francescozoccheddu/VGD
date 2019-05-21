using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.PlayerView;
using Wheeled.Networking;
using Wheeled.Networking.Client;
using Wheeled.Networking.Server;
using Wheeled.UI.Menu;

namespace Wheeled.Core
{
    public sealed partial class GameLauncher : MonoBehaviour
    {
        private static GameLauncher s_instance;

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


        public bool IsBusy => m_host?.IsStarted == true;
        public bool IsServer { get; private set; }

        public event GameRoomDiscoverEventHandler OnGameRoomDiscovered;

        private IGameHost m_host;

        private bool m_isQuitting = false;

        public void QuitGame()
        {
            if (m_host != null)
            {
                DestroyHost();
                if (!m_isQuitting)
                {
                    if (SceneManager.GetActiveScene().buildIndex != Scripts.Scenes.menuSceneBuildIndex)
                    {
                        SceneManager.LoadScene(Scripts.Scenes.menuSceneBuildIndex, LoadSceneMode.Single);
                    }
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                Debug.LogWarning("QuitGame has been ignored because no game is running or a loading is in progress");
            }
        }

        public void StartGameAsClient(IPEndPoint _endPoint)
        {
            if (!IsBusy)
            {
                EnsureClient().Start(_endPoint);
            }
            else
            {
                Debug.LogWarning("StartGameAsClient has been ignored because a game is running or loading");
            }
        }

        public void StartGameAsServer(GameRoomInfo _room)
        {
            if (!IsBusy)
            {
                EnsureServer().Start(_room);
                LoadScene(Scripts.Scenes.arenas[_room.arena].buildIndex);
            }
            else
            {
                Debug.LogWarning("StartGameAsServer has been ignored because a game is running or loading");
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

        private void DestroyHost()
        {
            UnregisterHostEvents();
            m_host?.Stop();
            m_host = null;
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

        private void GameStopped(EGameHostStopCause _cause)
        {
            ScreenManagerBehaviour.SetError(_cause.ToString());
            QuitGame();
        }

        private void LoadScene(int _scene)
        {
            SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single).completed += GameSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            m_isQuitting = true;
        }

        private void RoomDiscovered(GameRoomInfo _room)
        {
            OnGameRoomDiscovered?.Invoke(_room);
        }

        private void RoomJoined(GameRoomInfo _room)
        {
            LoadScene(Scripts.Scenes.arenas[_room.arena].buildIndex);
        }

        private void GameSceneLoaded(AsyncOperation _operation)
        {
            m_host?.GameReady();
            Cursor.lockState = CursorLockMode.Locked;
        }

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

    }

}