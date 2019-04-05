using System;
using System.Net;

namespace Wheeled.Networking.Server
{
    internal sealed partial class Server : IGameHost
    {
        private IGameManager m_game;

        private bool m_wasPlaying;

        public Server()
        {
            RoomInfo = null;
            IsStarted = false;
            m_game = null;
        }

        public event GameHostStopped OnStopped;

        public interface IGameManager
        {
            void ConnectedTo(NetworkManager.Peer _peer);

            void DisconnectedFrom(NetworkManager.Peer _peer);

            void LatencyUpdated(NetworkManager.Peer _peer, double _latency);

            void ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader);

            bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader);

            bool ShouldReplyToDiscoveryRequest();

            void Stopped();
        }

        public bool IsStarted { get; private set; }
        public GameRoomInfo? RoomInfo { get; private set; }

        public void Start(GameRoomInfo _room)
        {
            if (!IPAddress.IsLoopback(_room.endPoint.Address))
            {
                throw new ArgumentException("Server room must have loopback address");
            }
            NetworkManager.instance.listener = this;
            NetworkManager.instance.StartOnPort(_room.endPoint.Port);
            m_wasPlaying = true;
            IsStarted = true;
            RoomInfo = _room;
        }

        void IGameHost.GameReady()
        {
            if (m_game == null && IsStarted)
            {
                m_game = new ServerGameManager();
            }
        }

        void IGameHost.Stop()
        {
            Cleanup();
            NotifyStopped(GameHostStopCause.Programmatically);
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

        private void NotifyStopped(GameHostStopCause _cause)
        {
            if (m_wasPlaying)
            {
                m_wasPlaying = false;
                OnStopped?.Invoke(_cause);
            }
        }
    }
}