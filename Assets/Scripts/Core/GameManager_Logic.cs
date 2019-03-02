using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Assets.Scripts.Core;
using Wheeled.Networking;

namespace Wheeled.Core
{

    public sealed partial class GameManager
    {

        private readonly NetworkManager m_networkManager;

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

        public bool IsLoading { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsServer { get; private set; }

        public void StartGameAsServer(int _port)
        {
            if (!IsPlaying && !IsLoading)
            {
                IsPlaying = true;
                IsServer = true;
                m_networkManager.listener = null;
                m_networkManager.StartOnPort(_port);
                LoadScene(ScriptManager.Scenes.game[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsServer has been ignored because a game is running or loading");
            }
        }

        public void StartGameAsClient(GameRoomInfo _room)
        {
            if (!IsPlaying && !IsLoading)
            {
                IsPlaying = true;
                IsServer = true;
                Room = _room;
                m_networkManager.listener = null;
                m_networkManager.StartOnAvailablePort();
                LoadScene(ScriptManager.Scenes.game[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsClient has been ignored because a game is running or loading");
            }
        }

        public void StartServerDiscovery(int _port)
        {
            if (!IsPlaying && !IsLoading)
            {
                m_networkManager.StartOnAvailablePort();
                m_networkManager.StartDiscovery(_port);
            }
            else
            {
                Debug.LogWarning("DiscoveryServers has been ignored because a game is running or loading");
            }
        }

        public void QuitGame()
        {
            if (IsPlaying && !IsLoading)
            {
                m_networkManager.listener = null;
                m_networkManager.DisconnectAll();
                SceneManager.LoadScene(ScriptManager.Scenes.menu);
            }
            else
            {
                Debug.LogWarning("QuitGame has been ignored because no game is running or a loading is in progress");
            }
        }

        private void LoadScene(int _scene)
        {
            IsLoading = true;
            SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single).completed += OnSceneLoaded;
        }

        private void OnSceneLoaded(AsyncOperation _operation)
        {
            IsLoading = false;
            if (IsServer)
            {
                m_networkManager.listener = new Server(m_networkManager.instance);
            }
            else
            {

            }
        }

    }

}
