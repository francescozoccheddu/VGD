using LiteNetLib;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    internal sealed partial class Server : INetworkHost
    {

        private readonly NetManager m_netManager;

        public Server()
        {
            m_netManager = new NetManager(new NetEventHandler(this))
            {
                SimulateLatency = true,
                SimulationMaxLatency = 1500,
                DisconnectTimeout = 5000,
                DiscoveryEnabled = true,
            };
        }

        public PlayerEventHandler PlayerEvents { get; } = null;

        public bool IsRunning => m_netManager.IsRunning;

        public void Start(int _port)
        {
            if (!IsRunning)
            {
                m_netManager.Start(_port);
                if (IsRunning)
                {
                    m_localPlayer = GameManager.Instance.InstantiatePlayerBehaviour();
                    m_localPlayer.isInteractive = true;
                }
            }
            else
            {
                Debug.LogWarning("Start ignored because Server is already running");
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
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