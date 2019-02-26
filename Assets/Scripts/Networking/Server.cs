using LiteNetLib;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    internal sealed partial class Server : MonoBehaviour, INetworkHost
    {

        public const int c_port = 9050;

        private PlayerBehaviour m_localPlayer;
        private readonly Dictionary<NetPeer, PlayerBehaviour> m_players = new Dictionary<NetPeer, PlayerBehaviour>();

        private NetManager m_netManager;

        public NetworkHostHolder holder { get; set; }

        public void Start()
        {
            m_netManager = new NetManager(this)
            {
                SimulateLatency = true,
                SimulationMaxLatency = 1500,
                DisconnectTimeout = 5000,
                DiscoveryEnabled = false
            };
            if (!m_netManager.Start(c_port))
            {
                throw new NetworkException();
            }
            m_localPlayer = holder.InstatiatePlayer();
            m_localPlayer.isInteractive = true;
        }

        private void Update()
        {
            m_netManager.PollEvents();
        }

    }
}
