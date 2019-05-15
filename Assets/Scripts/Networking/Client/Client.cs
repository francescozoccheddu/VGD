using System.Net;
using Wheeled.Core.Data;

namespace Wheeled.Networking.Client
{
    public sealed partial class Client : IGameHost
    {
        public delegate void ConnectEventHandler(GameRoomInfo _room);

        public interface IGameManager
        {
            void LatencyUpdated(double _latency);

            void Received(Deserializer _reader);

            void Stopped();
        }

        public bool IsConnected { get; private set; }
        public bool IsPlaying => m_game != null;
        public bool IsStarted => m_server.IsValid;
        public GameRoomInfo? RoomInfo { get; private set; }

        public event ConnectEventHandler OnConnected;

        public event GameRoomDiscoverEventHandler OnRoomDiscovered;

        public event GameHostStopped OnStopped;

        private IGameManager m_game;

        private byte m_localPlayerId;

        private NetworkManager.Peer m_server;

        private bool m_wasStarted;

        public Client()
        {
            m_server = new NetworkManager.Peer();
            m_game = null;
            m_wasStarted = false;
            IsConnected = false;
            RoomInfo = null;
        }

        public void Start(IPEndPoint _endPoint)
        {
            if (IsPlaying)
            {
                ((IGameHost) this).Stop();
            }
            NetworkManager.instance.listener = this;
            NetworkManager.instance.StartOnAvailablePort();
            Serializer.WritePlayerInfo(PlayerPreferences.Info);
            m_server = NetworkManager.instance.ConnectTo(_endPoint, true);
            m_wasStarted = true;
        }

        public void StartRoomDiscovery(int _port)
        {
            NetworkManager.instance.listener = this;
            NetworkManager.instance.StartOnAvailablePort();
            NetworkManager.instance.StartDiscovery(_port, false);
        }

        void IGameHost.GameReady()
        {
            if (m_game == null && IsStarted)
            {
                m_game = new ClientGameManager(this, m_localPlayerId);
            }
        }

        void IGameHost.Stop()
        {
            Cleanup();
            NotifyStopped(EGameHostStopCause.Programmatically);
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

        private void NotifyStopped(EGameHostStopCause _cause)
        {
            if (m_wasStarted)
            {
                m_wasStarted = false;
                OnStopped?.Invoke(_cause);
            }
        }
    }
}