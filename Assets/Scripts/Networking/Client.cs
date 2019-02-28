using LiteNetLib;
using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    internal sealed partial class Client : INetworkHost
    {

        public delegate void GameRoomDiscoveredEventHandler(GameRoom room);

        public event GameRoomDiscoveredEventHandler GameRoomDiscovered;

        private readonly NetManager m_netManager;

        public Client()
        {
            m_netManager = new NetManager(new NetEventHandler(this))
            {
                DiscoveryEnabled = false,
            };
        }

        public void StartServerDiscovery(int _port)
        {
            m_netManager.SendDiscoveryRequest(new byte[0], _port);
        }

        public void Start()
        {
            m_netManager.Start();
            IsRunning = m_netManager.IsRunning;
        }

        public void Connect(GameRoom _room)
        {
            m_netManager.Connect(_room.remoteEndPoint, "dioporco");
        }

        public PlayerEventHandler PlayerEvents { get; } = null;

        public bool IsRunning { get; private set; } = false;

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                // Stop networking
                m_netManager.DisconnectAll();
                m_netManager.Stop();
                // Destroy players
                foreach (PlayerBehaviour pb in m_netPlayers.Values)
                {
                    pb.Destroy();
                }
                m_netPlayers.Clear();
                m_localPlayer?.Destroy();
                m_localPlayer = null;
            }
        }

        public void Update()
        {
            m_netManager.PollEvents();
        }
    }
}
