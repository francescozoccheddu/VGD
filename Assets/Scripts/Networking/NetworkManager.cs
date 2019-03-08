#define SINGLETON

using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets;


namespace Wheeled.Networking
{
    internal sealed class NetworkManager
    {

#if SINGLETON
        public static readonly NetworkManager instance = new NetworkManager();
#endif

        private const bool c_simulateBadNetwork = true;

        private sealed class NetEventHandler : INetEventListener
        {

            private readonly NetworkManager m_manager;

            public NetEventHandler(NetworkManager _manager)
            {
                m_manager = _manager;
            }

            public void OnConnectionRequest(ConnectionRequest _request)
            {
                if (m_manager.listener?.ShouldAcceptConnectionRequest(new Peer(_request.Peer), _request.Data) == true)
                {
                    _request.Accept();
                }
                else
                {
                    _request.Reject();
                }
            }

            public void OnNetworkError(IPEndPoint _endPoint, SocketError _socketError)
            {
                if (!m_manager.IsRunning)
                {
                    m_manager.NotifyStopped(StopCause.NetworkError);
                }
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
                m_manager.listener?.LatencyUpdated(new Peer(peer), latency);
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                m_manager.listener?.ReceivedFrom(new Peer(peer), reader);
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint _remoteEndPoint, NetPacketReader _reader, UnconnectedMessageType messageType)
            {
                if (messageType == UnconnectedMessageType.DiscoveryRequest)
                {
                    NetDataWriter writer = null;
                    if (m_manager.listener?.ShouldReplyToDiscoveryRequest(out writer) == true)
                    {
                        if (writer != null)
                        {
                            m_manager.m_netManager.SendDiscoveryResponse(writer, _remoteEndPoint);
                        }
                        else
                        {
                            m_manager.m_netManager.SendDiscoveryResponse(new byte[0], _remoteEndPoint);
                        }
                    }
                }
                else if (messageType == UnconnectedMessageType.DiscoveryResponse)
                {
                    m_manager.listener.Discovered(_remoteEndPoint, _reader);
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

            public object UserData => m_peer.Tag;

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

        public interface IEventListener
        {

            void ReceivedFrom(Peer _peer, NetPacketReader _reader);

            void DisconnectedFrom(Peer _peer);

            void ConnectedTo(Peer _peer);

            bool ShouldAcceptConnectionRequest(Peer _peer, NetDataReader _reader);

            bool ShouldReplyToDiscoveryRequest(out NetDataWriter _writer);

            void LatencyUpdated(Peer _peer, int _latency);

            void Discovered(IPEndPoint _endPoint, NetDataReader _reader);

            void Stopped(StopCause _cause);

        }

        public enum StopCause
        {
            UnableToStart, Programmatically, NetworkError, UnexpectedStop
        }

        private readonly NetManager m_netManager;
        private bool m_wasRunning;

        public bool IsRunning => m_netManager.IsRunning;
        public int Port => m_netManager.LocalPort;

        public IEventListener listener;

#if SINGLETON
        private NetworkManager()
#else
        public NetworkManager()
#endif
        {
            m_wasRunning = false;
            m_netManager = new NetManager(new NetEventHandler(this))
            {
                DiscoveryEnabled = true,
                SimulatePacketLoss = c_simulateBadNetwork,
                SimulationPacketLossChance = 20,
                SimulateLatency = c_simulateBadNetwork,
                SimulationMinLatency = 10,
                SimulationMaxLatency = 200
            };
        }

        private void NotifyStopped(StopCause _cause)
        {
            if (m_wasRunning)
            {
                m_wasRunning = false;
                listener?.Stopped(_cause);
            }
        }

        private void NotifyIfNotRunning()
        {
            if (!IsRunning)
            {
                NotifyStopped(StopCause.UnexpectedStop);
            }
        }

        public void StartDiscovery(int _port)
        {
            NotifyIfNotRunning();
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

        public Peer? ConnectTo(IPEndPoint _endPoint, NetDataWriter _writer)
        {
            NotifyIfNotRunning();
            if (IsRunning)
            {
                NetPeer p = m_netManager.Connect(_endPoint, _writer);
                return new Peer(p);
            }
            else
            {
                return null;
            }
        }

        public void DisconnectAll()
        {
            if (IsRunning)
            {
                m_netManager.DisconnectAll();
            }
            NotifyIfNotRunning();
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
            NotifyIfNotRunning();
        }

    }

}
