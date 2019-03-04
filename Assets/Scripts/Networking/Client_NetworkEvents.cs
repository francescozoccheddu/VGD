using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed partial class Client : IEventListener
    {

        private readonly NetworkInstance m_network;
        private readonly Player m_localPlayer;
        private readonly Dictionary<int, Player> m_netPlayers;
        private readonly Peer m_server;

        public delegate void DisconnectedEventHandler();

        public event DisconnectedEventHandler OnDisconnected;

        public Client(NetworkInstance _network, Peer _server)
        {
            m_network = _network;
            m_localPlayer = new Player();
            m_localPlayer.Setup(null, false, false);
            m_netPlayers = new Dictionary<int, Player>();
            m_server = _server;
        }

        public void ConnectedTo(Peer _peer)
        {
            if (_peer != m_server)
            {
                _peer.Disconnect();
                Debug.Log("Connected");
            }
        }

        public void DisconnectedFrom(Peer _peer)
        {
            if (m_server == _peer)
            {
                OnDisconnected?.Invoke();
            }
        }

        public void ReceivedFrom(Peer _peer, NetPacketReader _reader)
        {
            if (_peer == m_server)
            {
                m_localPlayer.Move(_reader.GetInt(), _reader.GetInputState(), _reader.GetSimulationState());
            }
        }

        public bool ShouldAcceptConnectionRequest(Peer _peer, NetDataReader _reader)
        {
            return false;
        }

        public bool ShouldReplyToDiscoveryRequest()
        {
            return false;
        }

    }

}
