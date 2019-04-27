using System.Net;

namespace Wheeled.Networking.Server
{
    internal sealed partial class Server : NetworkManager.IEventListener
    {
        #region Public Methods

        void NetworkManager.IEventListener.ConnectedTo(NetworkManager.Peer _peer)
        {
            m_game?.ConnectedTo(_peer);
        }

        void NetworkManager.IEventListener.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            m_game?.DisconnectedFrom(_peer);
        }

        void NetworkManager.IEventListener.Discovered(IPEndPoint _endPoint, Deserializer _reader)
        {
        }

        NetworkManager.DiscoveryRequestAction NetworkManager.IEventListener.DiscoveryRequested(Deserializer _reader)
        {
            if (m_game?.ShouldReplyToDiscoveryRequest() == true)
            {
                Serializer.WriteDiscoveryInfo(RoomInfo.Value.map);
                return NetworkManager.DiscoveryRequestAction.ReplyWithData;
            }
            else
            {
                return NetworkManager.DiscoveryRequestAction.Ignore;
            }
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, double _latency)
        {
            m_game?.LatencyUpdated(_peer, _latency);
        }

        void NetworkManager.IEventListener.ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader)
        {
            m_game?.ReceivedFrom(_peer, _reader);
        }

        bool NetworkManager.IEventListener.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
        {
            return m_game?.ShouldAcceptConnectionRequest(_peer, _reader) == true;
        }

        void NetworkManager.IEventListener.Stopped(NetworkManager.StopCause _cause)
        {
            Cleanup();
            NotifyStopped(GameHostStopCause.NetworkError);
        }

        #endregion Public Methods
    }
}