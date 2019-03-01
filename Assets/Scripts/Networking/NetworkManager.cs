using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;
using Wheeled.Core;

namespace Wheeled.Networking
{


    internal sealed class NetworkManager
    {

        private sealed class NetEventHandler : INetEventListener
        {

            private readonly NetworkManager m_manager;

            public NetEventHandler(NetworkManager _manager)
            {
                m_manager = _manager;
            }

            public void OnConnectionRequest(ConnectionRequest request)
            {
                throw new System.NotImplementedException();
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                throw new System.NotImplementedException();
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
                throw new System.NotImplementedException();
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                throw new System.NotImplementedException();
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                throw new System.NotImplementedException();
            }

            public void OnPeerConnected(NetPeer peer)
            {
                throw new System.NotImplementedException();
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
                throw new System.NotImplementedException();
            }
        }

        public interface IPeer
        {
            int Ping { get; }
            float TimeSinceLastPacket { get; }
            void Send(NetDataWriter _writer, DeliveryMethod _method);
            void Disconnect();
        }

        private sealed class Peer : IPeer
        {

            private readonly NetPeer m_peer;

            public Peer(NetPeer _peer)
            {
                m_peer = _peer;
            }

            public int Ping => m_peer.Ping;

            public float TimeSinceLastPacket => m_peer.TimeSinceLastPacket;

            public void Disconnect()
            {
                m_peer.Disconnect();
            }

            public override bool Equals(object obj)
            {
                Peer peer = obj as Peer;
                return peer != null && peer.m_peer == m_peer;
            }

            public override int GetHashCode()
            {
                return m_peer.GetHashCode();
            }

            public void Send(NetDataWriter _writer, DeliveryMethod _method)
            {
                m_peer.Send(_writer, _method);
            }

        }

        public sealed class NetworkInstance
        {
            private readonly NetworkManager m_manager;

            public NetworkInstance(NetworkManager _manager)
            {
                m_manager = _manager;
            }

            public IPeer ConnectTo(IPEndPoint _endPoint, NetDataWriter _writer)
            {
                NetPeer p = m_manager.m_netManager.Connect(_endPoint, _writer);
                return new Peer(p);
            }

        }

        public interface EventListener
        {

            void ReceivedFrom(IPeer _peer, NetPacketReader _reader);

            void DisconnectedFrom(IPeer _peer);

            void ConnectedTo(IPeer _peer);

            bool ShouldReplyToDiscoveryRequest();

        }

        public enum StopCause
        {
            UnableToStart
        }

        public delegate void StopEventHandler(StopCause cause);
        public delegate void GameRoomDiscoveredEventHandler(GameRoom room);

        private readonly NetManager m_netManager;

        public readonly NetworkInstance instance;

        public EventListener listener;

        public event StopEventHandler Stopped;

        public event GameRoomDiscoveredEventHandler GameRoomDiscovered;

        public NetworkManager()
        {
            m_netManager = new NetManager(new NetEventHandler(this));
        }

        public void StartDiscovery(int _port)
        {

        }

        public void StartOnPort(int port)
        {
        }

        public void StartOnAvailablePort()
        {
        }


        public void Update()
        {

        }

    }
}
