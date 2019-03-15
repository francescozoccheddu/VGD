namespace Wheeled.Networking.Client
{

    internal sealed partial class Client : IGameHost
    {

        public interface IGameManager
        {

            void LatencyUpdated(float _latency);

            void Received(Deserializer _reader);

            void Stopped();

        }

        public delegate void ConnectEventHandler(GameRoomInfo _room);

        private NetworkManager.Peer m_server;
        private bool m_wasStarted;
        private IGameManager m_game;

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
                ((IGameHost) this).Stop();
            }
            RoomInfo = _room;
            NetworkManager.instance.listener = this;
            NetworkManager.instance.StartOnAvailablePort();
            m_server = NetworkManager.instance.ConnectTo(_room.endPoint, false);
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

        public void StartRoomDiscovery(int _port)
        {
            NetworkManager.instance.listener = this;
            NetworkManager.instance.StartOnAvailablePort();
            NetworkManager.instance.StartDiscovery(_port, false);
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
                m_game = new ClientGameManager(this);
            }
        }

    }

}
