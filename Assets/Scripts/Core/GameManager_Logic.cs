using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Networking;

namespace Wheeled.Core
{
    public sealed partial class GameManager
    {

        public event GameRoomDiscoveredEventHandler GameRoomDiscovered
        {
            add
            {
                m_networkManager.GameRoomDiscovered += value;
            }
            remove
            {
                m_networkManager.GameRoomDiscovered -= value;
            }
        }

        public GameRoomInfo? Room { get; private set; }

        public bool IsPlaying { get; private set; }

        private readonly NetworkManager m_networkManager;

        public void StartGameAsServer(int _port)
        {
            if (!IsPlaying)
            {
                m_networkManager.StartOnPort(_port);
                IsPlaying = true;
                LoadScene(maps.gameScenes[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsServer has been ignored because a game is running");
            }
        }

        public void StartGameAsClient(GameRoomInfo _room)
        {
            if (!IsPlaying)
            {
                m_networkManager.StartOnAvailablePort();
                Room = _room;
                IsPlaying = true;
                LoadScene(maps.gameScenes[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsClient has been ignored because a game is running");
            }
        }

        public void StartServerDiscovery(int _port)
        {
            if (!IsPlaying)
            {
                m_networkManager.StartOnAvailablePort();
                m_networkManager.StartDiscovery(_port);
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
                m_networkManager.DisconnectAll();
                SceneManager.LoadScene(maps.menuScene);
            }
            else
            {
                Debug.LogWarning("QuitGame has been ignored because no game is running");
            }
        }

        private void LoadScene(int _scene)
        {
            SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single).completed += OnSceneLoaded;
        }

        private void OnSceneLoaded(AsyncOperation _operation)
        {
        }

    }
}
