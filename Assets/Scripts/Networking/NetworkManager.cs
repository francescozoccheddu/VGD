﻿#define SINGLETON

using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;


namespace Wheeled.Networking
{
    internal sealed partial class NetworkManager
    {

        private static readonly NetDataWriter s_emptyDataWriter = new NetDataWriter(false, 0);

#if SINGLETON
        public static readonly NetworkManager instance = new NetworkManager();
#endif

        private const bool c_simulateBadNetwork = true;

        public readonly struct Peer : IEquatable<Peer>
        {

            private readonly NetPeer m_peer;

            public Peer(NetPeer _peer = null)
            {
                m_peer = _peer;
            }

            public bool IsValid => m_peer != null;

            public int Ping => m_peer?.Ping ?? 0;

            public float TimeSinceLastPacket => m_peer?.TimeSinceLastPacket ?? 0.0f;

            public object UserData => m_peer?.Tag;

            public void Disconnect()
            {
                m_peer?.Disconnect();
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
                return _other.IsValid && IsValid && _other.m_peer.Id == m_peer.Id;
            }

            public override int GetHashCode()
            {
                return (m_peer?.Id)?.GetHashCode() ?? 0;
            }

            public void Send(NetDataWriter _writer, DeliveryMethod _method)
            {
                m_peer?.Send(_writer, _method);
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
            m_netManager = new NetManager(this)
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

        public Peer ConnectTo(IPEndPoint _endPoint, NetDataWriter _writer = null)
        {
            NotifyIfNotRunning();
            if (IsRunning)
            {
                return new Peer(m_netManager.Connect(_endPoint, _writer ?? s_emptyDataWriter));
            }
            else
            {
                return new Peer();
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
