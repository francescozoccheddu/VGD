﻿using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

namespace Wheeled.Networking.Client
{

    internal sealed partial class Client : Client.IServer, NetworkManager.IEventListener, IGameHost
    {

        public interface IServer
        {
            int Ping { get; }
            void Send(NetDataWriter _writer, DeliveryMethod _method);
        }

        #region Client.IServer

        int IServer.Ping => m_server.Ping;

        void IServer.Send(NetDataWriter _writer, DeliveryMethod _method)
        {
            m_server.Send(_writer, _method);
        }

        #endregion

        #region NetworkManager.IEventListener

        void NetworkManager.IEventListener.ConnectedTo(NetworkManager.Peer _peer)
        {
            if (_peer == m_server)
            {
                IsConnected = true;
                OnConnected?.Invoke(RoomInfo.Value);
            }
            else
            {
                _peer.Disconnect();
            }
        }

        void NetworkManager.IEventListener.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            if (_peer == m_server)
            {
                bool wasConnected = IsConnected;
                Cleanup();
                NotifyStopped(wasConnected ? GameHostStopCause.Disconnected : GameHostStopCause.UnableToConnect);
            }
        }

        void NetworkManager.IEventListener.Discovered(IPEndPoint _endPoint, NetDataReader _reader)
        {
            // TODO Parse info
            OnRoomDiscovered?.Invoke(new GameRoomInfo(_endPoint, "", 0));
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, int _latency)
        {
            if (_peer == m_server)
            {
                m_game?.LatencyUpdated(_latency);
            }
            else
            {
                _peer.Disconnect();
            }
        }

        void NetworkManager.IEventListener.ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
            if (_peer == m_server)
            {
                m_game?.Received(_reader);
            }
            else
            {
                _peer.Disconnect();
            }
        }

        bool NetworkManager.IEventListener.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            return false;
        }

        bool NetworkManager.IEventListener.ShouldReplyToDiscoveryRequest(out NetDataWriter _writer)
        {
            _writer = null;
            return false;
        }

        void NetworkManager.IEventListener.Stopped(NetworkManager.StopCause _cause)
        {
            NotifyStopped(GameHostStopCause.NetworkError);
        }

        #endregion

    }

}