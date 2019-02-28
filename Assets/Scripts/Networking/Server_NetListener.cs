using LiteNetLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    internal sealed partial class Server
    {

        private sealed class NetEventHandler : INetEventListener
        {

            private readonly Server m_server;

            public NetEventHandler(Server _server)
            {
                m_server = _server;
            }

            public void OnConnectionRequest(ConnectionRequest request)
            {
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                GameManager.Instance.QuitGame();
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                if (messageType == UnconnectedMessageType.DiscoveryRequest)
                {
                    m_server.m_netManager.SendDiscoveryResponse(new byte[0], remoteEndPoint);
                }
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
