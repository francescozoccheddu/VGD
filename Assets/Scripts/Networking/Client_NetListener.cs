using LiteNetLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{
    public sealed partial class Client : INetEventListener
    {

        public readonly Dictionary<NetPeer, PlayerBehaviour> m_players = new Dictionary<NetPeer, PlayerBehaviour>();

        public void OnConnectionRequest(ConnectionRequest request)
        {
            // TODO Prevent being attacked
            request.Reject();
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (peer == m_server)
            {
                MessageType type = reader.GetEnum<MessageType>();

            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                RoomDiscovered?.Invoke(
                    new GameRoom
                    {
                        endPoint = remoteEndPoint
                    }
                );
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
            m_server = peer;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (peer == m_server)
            {
                m_server = null;
            }
        }
    }
}
