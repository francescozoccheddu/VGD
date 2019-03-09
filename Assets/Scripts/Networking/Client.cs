using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using UnityEngine;

namespace Wheeled.Networking
{

    internal sealed partial class Client : IGameHost
    {

        public interface IGameManager
        {

            void LatencyUpdated(int _latency);

            void Received(NetDataReader _reader);

            void Stopped();

        }

        public sealed class Server
        {

            public int Ping => 0;

            public void Send(NetDataWriter _writer)
            {

            }

        }

        private sealed class NetEventListener : NetworkManager.IEventListener
        {

            private readonly Client m_client;

            public NetEventListener(Client _client)
            {
                Debug.Assert(_client != null);
                m_client = _client;
            }

            public void ConnectedTo(NetworkManager.Peer _peer)
            {
                if (_peer == m_client.m_server)
                {
                    m_client.IsConnected = true;
                    m_client.OnConnected?.Invoke(m_client.RoomInfo.Value);
                }
                else
                {
                    _peer.Disconnect();
                }
            }

            public void DisconnectedFrom(NetworkManager.Peer _peer)
            {
                if (_peer == m_client.m_server)
                {
                    bool wasConnected = m_client.IsConnected;
                    m_client.Cleanup();
                    m_client.NotifyStopped(wasConnected ? GameHostStopCause.Disconnected : GameHostStopCause.UnableToConnect);
                }
            }

            public void Discovered(IPEndPoint _endPoint, NetDataReader _reader)
            {
                // TODO Parse info
                m_client.OnRoomDiscovered?.Invoke(new GameRoomInfo(_endPoint, "", 0));
            }

            public void LatencyUpdated(NetworkManager.Peer _peer, int _latency)
            {
                // TODO
            }

            public void ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
            {
                // TODO
            }

            public bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
            {
                return false;
            }

            public bool ShouldReplyToDiscoveryRequest(out NetDataWriter _writer)
            {
                _writer = null;
                return false;
            }

            public void Stopped(NetworkManager.StopCause _cause)
            {
                m_client.NotifyStopped(GameHostStopCause.NetworkError);
            }

        }

        public delegate void ConnectEventHandler(GameRoomInfo _room);

        private readonly NetEventListener m_netListener;
        private NetworkManager.Peer m_server;
        private bool m_wasStarted;
        private ClientGameManager m_game;

        public bool IsStarted => m_server.IsValid;
        public bool IsPlaying => m_game != null;
        public bool IsConnected { get; private set; }
        public GameRoomInfo? RoomInfo { get; private set; }

        public event GameRoomDiscoverEventHandler OnRoomDiscovered;
        public event ConnectEventHandler OnConnected;
        public event GameHostStopped OnStopped;

        private void NotifyStopped(GameHostStopCause _cause)
        {
            if (m_wasStarted)
            {
                m_wasStarted = false;
                OnStopped?.Invoke(_cause);
            }
        }

        public Client()
        {
            m_netListener = new NetEventListener(this);
            m_server = new NetworkManager.Peer();
            m_game = null;
            m_wasStarted = false;
            IsConnected = false;
            RoomInfo = null;
        }

        public void Start(GameRoomInfo _room)
        {
            if (IsPlaying)
            {
                Stop();
            }
            RoomInfo = _room;
            NetworkManager.instance.listener = m_netListener;
            NetworkManager.instance.StartOnAvailablePort();
            m_server = NetworkManager.instance.ConnectTo(_room.endPoint);
            m_wasStarted = true;
        }

        private void Cleanup()
        {
            m_server.Disconnect();
            m_server = new NetworkManager.Peer();
            m_game?.Stopped();
            m_game = null;
            IsConnected = false;
            RoomInfo = null;
        }

        public void Stop()
        {
            Cleanup();
            NotifyStopped(GameHostStopCause.Programmatically);
        }

        public void StartRoomDiscovery(int _port)
        {
            NetworkManager.instance.listener = m_netListener;
            NetworkManager.instance.StartOnAvailablePort();
            NetworkManager.instance.StartDiscovery(_port);
        }

        public void GameReady()
        {
            if (m_game == null && IsStarted)
            {
                m_game = new ClientGameManager();
            }
        }
    }

}
