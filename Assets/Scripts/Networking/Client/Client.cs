using System.Net;
using Wheeled.Core.Data;

namespace Wheeled.Networking.Client
{
    internal sealed partial class Client : IGameHost
    {
        #region Public Delegates

        public delegate void ConnectEventHandler(GameRoomInfo _room);

        #endregion Public Delegates

        #region Public Interfaces

        public interface IGameManager
        {
            #region Public Methods

            void LatencyUpdated(double _latency);

            void Received(Deserializer _reader);

            void Stopped();

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Public Properties

        public bool IsConnected { get; private set; }
        public bool IsPlaying => m_game != null;
        public bool IsStarted => m_server.IsValid;
        public GameRoomInfo? RoomInfo { get; private set; }

        #endregion Public Properties

        #region Public Events

        public event ConnectEventHandler OnConnected;

        public event GameRoomDiscoverEventHandler OnRoomDiscovered;

        public event GameHostStopped OnStopped;

        #endregion Public Events

        #region Private Fields

        private IGameManager m_game;

        private byte m_localPlayerId;

        private NetworkManager.Peer m_server;

        private bool m_wasStarted;

        #endregion Private Fields

        #region Public Constructors

        public Client()
        {
            m_server = new NetworkManager.Peer();
            m_game = null;
            m_wasStarted = false;
            IsConnected = false;
            RoomInfo = null;
        }

        #endregion Public Constructors

        #region Public Methods

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
            NotifyStopped(GameHostStopCause.Programmatically);
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            m_server.Disconnect();
            m_server = new NetworkManager.Peer();
            m_game?.Stopped();
            m_game = null;
            IsConnected = false;
            RoomInfo = null;
        }

        private void NotifyStopped(GameHostStopCause _cause)
        {
            if (m_wasStarted)
            {
                m_wasStarted = false;
                OnStopped?.Invoke(_cause);
            }
        }

        #endregion Private Methods
    }
}