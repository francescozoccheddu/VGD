using LiteNetLib;
using UnityEngine;
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
            Debug.Log("Sent DiscoveryRequest");
            m_netManager.SendDiscoveryRequest(new byte[] { 8 }, _port);
        }

        public void Start()
        {
            if (!IsRunning)
            {
                m_netManager.Start();
                Debug.LogFormat("Client started on port {0}", m_netManager.LocalPort);
            }
            else
            {
                Debug.LogWarning("Start ignored because Client is already running");
            }
        }

        public void Connect(GameRoom _room)
        {
            if (IsRunning)
            {
                m_netManager.Connect(_room.remoteEndPoint, "dioporco");
            }
            else
            {
                Debug.LogWarning("Connect ignored because Client is not running");
            }
        }

        public PlayerEventHandler PlayerEvents { get; } = null;

        public bool IsRunning => m_netManager.IsRunning;

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
