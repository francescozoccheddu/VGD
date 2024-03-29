﻿using System.Net;

namespace Wheeled.Networking.Server
{
    public sealed partial class Server : NetworkManager.IEventListener
    {
        void NetworkManager.IEventListener.ConnectedTo(NetworkManager.Peer _peer)
        {
            ((IGameManager) m_game)?.ConnectedTo(_peer);
        }

        void NetworkManager.IEventListener.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            ((IGameManager) m_game)?.DisconnectedFrom(_peer);
        }

        void NetworkManager.IEventListener.Discovered(IPEndPoint _endPoint, Deserializer _reader)
        {
        }

        NetworkManager.EDiscoveryRequestAction NetworkManager.IEventListener.DiscoveryRequested(Deserializer _reader)
        {
            if (((IGameManager) m_game)?.ShouldReplyToDiscoveryRequest() == true)
            {
                Serializer.WriteDiscoveryInfo(RoomInfo.Value.arena);
                return NetworkManager.EDiscoveryRequestAction.ReplyWithData;
            }
            else
            {
                return NetworkManager.EDiscoveryRequestAction.Ignore;
            }
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, double _latency)
        {
            ((IGameManager) m_game)?.LatencyUpdated(_peer, _latency);
        }

        void NetworkManager.IEventListener.ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader)
        {
            ((IGameManager) m_game)?.ReceivedFrom(_peer, _reader);
        }

        bool NetworkManager.IEventListener.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
        {
            return ((IGameManager) m_game)?.ShouldAcceptConnectionRequest(_peer, _reader) == true;
        }

        void NetworkManager.IEventListener.Stopped(NetworkManager.EStopCause _cause)
        {
            Cleanup();
            NotifyStopped(EGameHostStopCause.NetworkError);
        }
    }
}