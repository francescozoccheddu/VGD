using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Assets.Scripts.Core;
using Wheeled.Networking;

namespace Wheeled.Core
{

    public sealed partial class GameManager
    {

        private readonly NetworkManager m_networkManager;

        public event GameRoomDiscoveredEventHandler OnGameRoomDiscovered
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

        private NetworkManager.Peer? m_serverPeer;

        public void StartGameAsServer(int _port)
        {
            if (!IsPlaying && !IsLoading)
            {
                IsPlaying = true;
                IsServer = true;
                DestroyHost();
                m_networkManager.StartOnPort(_port);
                LoadScene(ScriptManager.Scenes.game[0]);
            }
            else
            {
                Debug.LogWarning("StartGameAsServer has been ignored because a game is running or loading");
            }
        }

        private sealed class ClientConnectionHelper : NetworkManager.IEventListener
        {

            public void ConnectedTo(NetworkManager.Peer _peer)
            {
                if (_peer == Instance.m_serverPeer)
                {
                    Instance.LoadScene(ScriptManager.Scenes.game[0]);
                }
            }

            public void DisconnectedFrom(NetworkManager.Peer _peer)
            {
            }

            public void ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
            {
            }

            public bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
            {
                return false;
            }

            public bool ShouldReplyToDiscoveryRequest()
            {
                return false;
            }
        }

        public void StartGameAsClient(GameRoomInfo _room)
        {
            if (!IsPlaying && !IsLoading)
            {
                IsPlaying = true;
                IsServer = false;
                IsLoading = true;
                Room = _room;
                DestroyHost();
                m_networkManager.StartOnAvailablePort();
                m_networkManager.listener = new ClientConnectionHelper();
                m_serverPeer = m_networkManager.instance.ConnectTo(_room.remoteEndPoint, new LiteNetLib.Utils.NetDataWriter());
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
                IsPlaying = false;
                DestroyHost();
                m_networkManager.DisconnectAll();
                LoadScene(ScriptManager.Scenes.menu);
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

        private void DestroyHost()
        {
            NetworkManager.IEventListener host = m_networkManager.listener;
            m_networkManager.listener = null;
            Client client = host as Client;
            if (client != null)
            {
                client.OnDisconnected -= ClientDisconnected;
            }
        }

        private void OnSceneLoaded(AsyncOperation _operation)
        {
            IsLoading = false;
            if (IsPlaying)
            {
                if (IsServer)
                {
                    m_networkManager.listener = new Server(m_networkManager.instance);
                }
                else
                {
                    Client client = new Client(m_networkManager.instance, (NetworkManager.Peer) m_serverPeer);
                    client.OnDisconnected += ClientDisconnected;
                    m_networkManager.listener = client;
                }
            }
        }

        private void ClientDisconnected()
        {
            QuitGame();
        }

    }

}
