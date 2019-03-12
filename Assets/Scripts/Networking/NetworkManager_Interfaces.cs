using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;

namespace Wheeled.Networking
{
    internal sealed partial class NetworkManager : INetEventListener
    {

        #region INetEventListener

        void INetEventListener.OnConnectionRequest(ConnectionRequest _request)
        {
            if (listener?.ShouldAcceptConnectionRequest(new Peer(_request.Peer), _request.Data) == true)
            {
                _request.Accept();
            }
            else
            {
                _request.Reject();
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint _endPoint, SocketError _socketError)
        {
            if (!IsRunning)
            {
                NotifyStopped(StopCause.NetworkError);
            }
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer _peer, int _latency)
        {
            listener?.LatencyUpdated(new Peer(_peer), _latency / 1000.0f);
        }

        void INetEventListener.OnNetworkReceive(NetPeer _peer, NetPacketReader _reader, DeliveryMethod _deliveryMethod)
        {
            listener?.ReceivedFrom(new Peer(_peer), _reader);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint _remoteEndPoint, NetPacketReader _reader, UnconnectedMessageType _messageType)
        {
            if (_messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                NetDataWriter writer = null;
                if (listener?.ShouldReplyToDiscoveryRequest(out writer) == true)
                {
                    if (writer != null)
                    {
                        m_netManager.SendDiscoveryResponse(writer, _remoteEndPoint);
                    }
                    else
                    {
                        m_netManager.SendDiscoveryResponse(new byte[0], _remoteEndPoint);
                    }
                }
            }
            else if (_messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                listener.Discovered(_remoteEndPoint, _reader);
            }
        }

        void INetEventListener.OnPeerConnected(NetPeer _peer)
        {
            listener?.ConnectedTo(new Peer(_peer));
        }

        void INetEventListener.OnPeerDisconnected(NetPeer _peer, DisconnectInfo _disconnectInfo)
        {
            listener?.DisconnectedFrom(new Peer(_peer));
        }
    }

    #endregion

}

