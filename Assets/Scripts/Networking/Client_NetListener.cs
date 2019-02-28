﻿using LiteNetLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    internal sealed partial class Client
    {

        private sealed class NetEventHandler : INetEventListener
        {

            private readonly Client m_client;

            public NetEventHandler(Client _client)
            {
                m_client = _client;
            }

            public void OnConnectionRequest(ConnectionRequest request)
            {
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
            }

            public void OnPeerConnected(NetPeer peer)
            {
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
            }

        }

        private readonly Dictionary<NetPeer, PlayerBehaviour> m_netPlayers = new Dictionary<NetPeer, PlayerBehaviour>();
        private PlayerBehaviour m_localPlayer;

    }
}
