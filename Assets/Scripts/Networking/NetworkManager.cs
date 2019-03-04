using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using Wheeled.Core;

namespace Wheeled.Networking
{

    public delegate void GameRoomDiscoveredEventHandler(GameRoomInfo room);

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
                if (m_manager.listener?.ShouldAcceptConnectionRequest(new Peer(request.Peer), request.Data) == true)
                {
                    request.Accept();
                }
                else
                {
                    request.Reject();
                }
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                if (!m_manager.IsRunning)
                {
                    m_manager.NotifyStopped(StopCause.NetworkError);
                }
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
                // TODO Should forward event?
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                m_manager.listener?.ReceivedFrom(new Peer(peer), reader);
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                if (messageType == UnconnectedMessageType.DiscoveryRequest)
                {
                    if (m_manager.listener?.ShouldReplyToDiscoveryRequest() == true)
                    {
                        // TODO Add support for replying with room info
                        m_manager.m_netManager.SendDiscoveryResponse(new byte[] { 0 }, remoteEndPoint);
                    }
                }
                else if (messageType == UnconnectedMessageType.DiscoveryResponse)
                {
                    // TODO Parse discovery response to get room info
                    m_manager.GameRoomDiscovered?.Invoke(new GameRoomInfo
                    {
                        remoteEndPoint = remoteEndPoint
                    });
                }
            }

            public void OnPeerConnected(NetPeer peer)
            {
                m_manager.listener?.ConnectedTo(new Peer(peer));
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
                m_manager.listener?.DisconnectedFrom(new Peer(peer));
            }
        }

        public readonly struct Peer : IEquatable<Peer>
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

            public static bool operator ==(Peer _a, Peer _b)
            {
                return _a.Equals(_b);
            }

            public static bool operator !=(Peer _a, Peer _b)
            {
                return !(_a == _b);
            }

            public override bool Equals(object _other)
            {
                return (_other as Peer?)?.Equals(this) == true;
            }

            public bool Equals(Peer _other)
            {
                return _other.m_peer.Id == m_peer.Id;
            }

            public override int GetHashCode()
            {
                return m_peer.Id.GetHashCode();
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

            public Peer ConnectTo(IPEndPoint _endPoint, NetDataWriter _writer)
            {
                NetPeer p = m_manager.m_netManager.Connect(_endPoint, _writer);
                return new Peer(p);
            }

        }

        public interface IEventListener
        {

            void ReceivedFrom(Peer _peer, NetPacketReader _reader);

            void DisconnectedFrom(Peer _peer);

            void ConnectedTo(Peer _peer);

            bool ShouldAcceptConnectionRequest(Peer _peer, NetDataReader _reader);

            bool ShouldReplyToDiscoveryRequest();

        }

        public enum StopCause
        {
            UnableToStart, Programmatically, NetworkError, UnexpectedStop
        }

        public delegate void StopEventHandler(StopCause cause);

        private readonly NetManager m_netManager;
        private bool m_wasRunning;

        public readonly NetworkInstance instance;
        public bool IsRunning => m_netManager.IsRunning;
        public int Port => m_netManager.LocalPort;

        public IEventListener listener;

        public event StopEventHandler Stopped;

        public event GameRoomDiscoveredEventHandler GameRoomDiscovered;

        public NetworkManager()
        {
            m_netManager = new NetManager(new NetEventHandler(this))
            {
                DiscoveryEnabled = true,
                UpdateTime = 500,
            };
            m_wasRunning = false;
            instance = new NetworkInstance(this);
        }

        private void NotifyStopped(StopCause _cause)
        {
            if (m_wasRunning)
            {
                m_wasRunning = false;
                Stopped?.Invoke(_cause);
            }
        }

        public void StartDiscovery(int _port)
        {
            if (IsRunning)
            {
                m_netManager.SendDiscoveryRequest(new byte[0], _port);
            }
        }

        public void StartOnPort(int _port)
        {
            m_wasRunning = true;
            if (Port != _port)
            {
                Stop();
            }
            m_netManager.Start(_port);
            if (!IsRunning)
            {
                NotifyStopped(StopCause.UnableToStart);
            }
        }

        public void StartOnAvailablePort()
        {
            m_wasRunning = true;
            if (!IsRunning)
            {
                m_netManager.Start();
            }
            if (!IsRunning)
            {
                NotifyStopped(StopCause.UnableToStart);
            }
        }

        public void DisconnectAll()
        {
            if (IsRunning)
            {
                m_netManager.DisconnectAll();
            }
            if (!IsRunning)
            {
                NotifyStopped(StopCause.UnexpectedStop);
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                m_netManager.Stop();
                NotifyStopped(StopCause.Programmatically);
            }
        }

        public void Update()
        {
            m_netManager.PollEvents();
            if (!IsRunning)
            {
                NotifyStopped(StopCause.UnexpectedStop);
            }
        }

    }

}
