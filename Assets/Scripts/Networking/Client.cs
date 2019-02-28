using LiteNetLib;
using System.Net;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    internal sealed partial class Client : INetworkHost
    {

        public struct GameRoom
        {
            public IPEndPoint endPoint;
            public string name;
            public int map;
        }

        public delegate void GameRoomDiscoveredEventHandler(GameRoom room);

        public event GameRoomDiscoveredEventHandler GameRoomDiscovered;

        private readonly NetManager m_netManager;

        public Client()
        {
            m_netManager = new NetManager(new NetEventHandler(this));
        }

        public void StartServerDiscovery()
        {

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
