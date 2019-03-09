using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Diagnostics;
using System.Net;

namespace Wheeled.Networking
{

    internal sealed class Server : IGameHost
    {

        public interface IGameManager
        {

            void ConnectedTo(NetworkManager.Peer _peer);

            void DisconnectedFrom(NetworkManager.Peer _peer);

            void LatencyUpdated(NetworkManager.Peer _peer, int _latency);

            void ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader);

            bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader);

            bool ShouldReplyToDiscoveryRequest();

            void Stopped();

        }

        private sealed class NetEventListener : NetworkManager.IEventListener
        {

            private readonly Server m_server;

            public NetEventListener(Server _server)
            {
                Debug.Assert(_server != null);
                m_server = _server;
            }

            public void ConnectedTo(NetworkManager.Peer _peer)
            {
                m_server.m_game?.ConnectedTo(_peer);
            }

            public void DisconnectedFrom(NetworkManager.Peer _peer)
            {
                m_server.m_game?.DisconnectedFrom(_peer);
            }

            public void Discovered(IPEndPoint _endPoint, NetDataReader _reader)
            {
            }

            public void LatencyUpdated(NetworkManager.Peer _peer, int _latency)
            {
                m_server.m_game?.LatencyUpdated(_peer, _latency);
            }

            public void ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
            {
                m_server.m_game?.ReceivedFrom(_peer, _reader);
            }

            public bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
            {
                return m_server.m_game?.ShouldAcceptConnectionRequest(_peer, _reader) == true;
            }

            public bool ShouldReplyToDiscoveryRequest(out NetDataWriter _outWriter)
            {
                if (m_server.m_game?.ShouldReplyToDiscoveryRequest() == true)
                {
                    _outWriter = new NetDataWriter();
                    // TODO Inject room data
                    return true;
                }
                else
                {
                    _outWriter = null;
                    return false;
                }
            }

            public void Stopped(NetworkManager.StopCause _cause)
            {
                m_server.Cleanup();
                m_server.NotifyStopped(GameHostStopCause.NetworkError);
            }

        }

        private readonly NetEventListener m_netListener;
        private bool m_wasPlaying;
        private ServerGameManager m_game;
        public GameRoomInfo? RoomInfo { get; private set; }

        public bool IsStarted { get; private set; }

        public event GameHostStopped OnStopped;

        private void NotifyStopped(GameHostStopCause _cause)
        {
            if (m_wasPlaying)
            {
                m_wasPlaying = false;
                OnStopped?.Invoke(_cause);
            }
        }

        public Server()
        {
            m_netListener = new NetEventListener(this);
            RoomInfo = null;
            IsStarted = false;
            m_game = null;
        }

        public void Start(GameRoomInfo _room)
        {
            if (!IPAddress.IsLoopback(_room.endPoint.Address))
            {
                throw new ArgumentException("Server room must have loopback address");
            }
            NetworkManager.instance.listener = m_netListener;
            NetworkManager.instance.StartOnPort(_room.endPoint.Port);
            m_wasPlaying = true;
            IsStarted = true;
            RoomInfo = _room;
        }

        private void Cleanup()
        {
            NetworkManager.instance.listener = null;
            NetworkManager.instance.DisconnectAll();
            m_game?.Stopped();
            m_game = null;
            IsStarted = false;
            RoomInfo = null;
        }

        public void Stop()
        {
            Cleanup();
            NotifyStopped(GameHostStopCause.Programmatically);
        }

        public void GameReady()
        {
            if (m_game == null && IsStarted)
            {
                m_game = new ServerGameManager();
            }
        }
    }

}
