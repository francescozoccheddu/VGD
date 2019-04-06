using LiteNetLib;
using LiteNetLib.Utils;

using System;
using System.Net;

using UnityEngine;

namespace Wheeled.Networking
{
    internal sealed partial class NetworkManager
    {
        public static readonly NetworkManager instance = new NetworkManager();
        public IEventListener listener;
        private static readonly NetDataWriter s_emptyDataWriter = new NetDataWriter(false, 0);
        private readonly NetManager m_netManager;

        private bool m_wasRunning;

        private NetworkManager()
        {
            m_wasRunning = false;
            m_netManager = new NetManager(this)
            {
                DiscoveryEnabled = true,
                SimulatePacketLoss = false,
                SimulationPacketLossChance = 20,
                SimulateLatency = false,
                SimulationMinLatency = 10,
                SimulationMaxLatency = 200,
            };
        }

        public enum DiscoveryRequestAction
        {
            Ignore, Reply, ReplyWithData
        }

        public enum SendMethod
        {
            Unreliable = LiteNetLib.DeliveryMethod.Unreliable,
            Sequenced = LiteNetLib.DeliveryMethod.Sequenced,
            ReliableUnordered = LiteNetLib.DeliveryMethod.ReliableUnordered,
            ReliableSequenced = LiteNetLib.DeliveryMethod.ReliableSequenced,
            ReliableOrdered = LiteNetLib.DeliveryMethod.ReliableOrdered
        }

        public enum StopCause
        {
            UnableToStart, Programmatically, NetworkError, UnexpectedStop
        }

        public interface IEventListener
        {
            void ConnectedTo(Peer _peer);

            void DisconnectedFrom(Peer _peer);

            void Discovered(IPEndPoint _endPoint, Deserializer _reader);

            DiscoveryRequestAction DiscoveryRequested(Deserializer _reader);

            void LatencyUpdated(Peer _peer, double _latency);

            void ReceivedFrom(Peer _peer, Deserializer _reader);

            bool ShouldAcceptConnectionRequest(Peer _peer, Deserializer _reader);

            void Stopped(StopCause _cause);
        }

        public int Port => m_netManager.LocalPort;

        public bool IsRunning => m_netManager.IsRunning;

        public Peer ConnectTo(IPEndPoint _endPoint, bool _sendData)
        {
            NotifyIfNotRunning();
            if (IsRunning)
            {
                return new Peer(m_netManager.Connect(_endPoint, _sendData ? Serializer.writer : s_emptyDataWriter));
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

        public void StartDiscovery(int _port, bool _sendData)
        {
            NotifyIfNotRunning();
            if (IsRunning)
            {
                if (_sendData)
                {
                    m_netManager.SendDiscoveryRequest(new byte[0], _port);
                }
                else
                {
                    m_netManager.SendDiscoveryRequest(Serializer.writer, _port);
                }
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
            if (Debug.isDebugBuild)
            {
                if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
                {
                    m_netManager.SimulatePacketLoss = m_netManager.SimulateLatency = !m_netManager.SimulateLatency;
                    Debugging.Printer.Print("BadNetwork", m_netManager.SimulatePacketLoss);
                }
            }
            m_netManager.PollEvents();
            NotifyIfNotRunning();
        }

        private void NotifyIfNotRunning()
        {
            if (!IsRunning)
            {
                NotifyStopped(StopCause.UnexpectedStop);
            }
        }

        private void NotifyStopped(StopCause _cause)
        {
            if (m_wasRunning)
            {
                m_wasRunning = false;
                listener?.Stopped(_cause);
            }
        }

        public readonly struct Peer : IEquatable<Peer>
        {
            private readonly NetPeer m_peer;

            public Peer(NetPeer _peer = null)
            {
                m_peer = _peer;
            }

            public bool IsValid => m_peer != null;

            public object UserData => m_peer?.Tag;
            public double Ping => m_peer?.Ping / 1000.0 ?? 0.0;

            public float TimeSinceLastPacket => m_peer?.TimeSinceLastPacket ?? 0.0f;

            public static bool operator !=(Peer _a, Peer _b)
            {
                return !(_a == _b);
            }

            public static bool operator ==(Peer _a, Peer _b)
            {
                return _a.Equals(_b);
            }

            public void Disconnect()
            {
                m_peer?.Disconnect();
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

            public void Send(NetworkManager.SendMethod _method)
            {
                m_peer?.Send(Serializer.writer, (DeliveryMethod) _method);
            }
        }
    }
}