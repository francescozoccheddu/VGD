using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using UnityEngine;

namespace Wheeled.Networking
{
    public sealed partial class Client : MonoBehaviour
    {

        public struct GameRoom
        {
            public IPEndPoint endPoint;
            public string name;
            public int map;
        }

        public delegate void GameRoomDiscoveredEventHandler(GameRoom room);

        private readonly NetDataWriter m_dataWriter = new NetDataWriter(false, 128);
        private NetManager m_netManager;
        private NetPeer m_server;
        private IPEndPoint m_desiredRoomEndPoint;

        // TODO Joined event
        // TODO Quit event
        public event GameRoomDiscoveredEventHandler RoomDiscovered;

        public bool IsConnected => m_server != null;

        public void DiscoverRooms()
        {
            //m_netManager.SendDiscoveryRequest(new byte[0], NetServer.port);
        }

        public void QuitRoom()
        {
            if (IsConnected)
            {
                m_server.Disconnect();
                m_server = null;
            }
        }

        public void JoinRoom(IPEndPoint endPoint)
        {
            QuitRoom();
            m_desiredRoomEndPoint = endPoint;
            m_netManager.Connect(endPoint, "changeme");
        }

        public void JoinRoom(GameRoom room)
        {
            JoinRoom(room.endPoint);
        }

        public void CancelJoinRoom()
        {
            m_desiredRoomEndPoint = null;
        }

        public void Start()
        {
            m_netManager = new NetManager(this)
            {
                SimulateLatency = true,
                SimulationMaxLatency = 1500,
                DisconnectTimeout = 5000,
            };
            if (!m_netManager.Start())
            {
                throw new NetworkException();
            }
        }

        public void Update()
        {
            m_netManager?.PollEvents();
        }

        public void OnDestroy()
        {
            m_netManager?.Stop();
        }

    }
}
