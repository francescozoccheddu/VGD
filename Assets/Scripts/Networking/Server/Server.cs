using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;

namespace Wheeled.Networking.Server
{

    internal sealed partial class Server : IGameHost
    {

        public interface IGameManager
        {

            void ConnectedTo(NetworkManager.Peer _peer);

            void DisconnectedFrom(NetworkManager.Peer _peer);

            void LatencyUpdated(NetworkManager.Peer _peer, float _latency);

            void ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader);

            bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader);

            bool ShouldReplyToDiscoveryRequest();

            void Stopped();

        }

        private bool m_wasPlaying;
        private IGameManager m_game;
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
            NetworkManager.instance.listener = this;
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

        void IGameHost.Stop()
        {
            Cleanup();
            NotifyStopped(GameHostStopCause.Programmatically);
        }

        void IGameHost.GameReady()
        {
            if (m_game == null && IsStarted)
            {
                m_game = new ServerGameManager();
            }
        }
    }

}
