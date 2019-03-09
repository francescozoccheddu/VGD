using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

namespace Wheeled.Networking
{

    internal sealed partial class Server : NetworkManager.IEventListener
    {

        #region NetworkManager.IEventListener

        void NetworkManager.IEventListener.ConnectedTo(NetworkManager.Peer _peer)
        {
            m_game?.ConnectedTo(_peer);
        }

        void NetworkManager.IEventListener.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            m_game?.DisconnectedFrom(_peer);
        }

        void NetworkManager.IEventListener.Discovered(IPEndPoint _endPoint, NetDataReader _reader)
        {
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, int _latency)
        {
            m_game?.LatencyUpdated(_peer, _latency);
        }

        void NetworkManager.IEventListener.ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
            m_game?.ReceivedFrom(_peer, _reader);
        }

        bool NetworkManager.IEventListener.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            return m_game?.ShouldAcceptConnectionRequest(_peer, _reader) == true;
        }

        bool NetworkManager.IEventListener.ShouldReplyToDiscoveryRequest(out NetDataWriter _outWriter)
        {
            if (m_game?.ShouldReplyToDiscoveryRequest() == true)
            {
                _outWriter = new NetDataWriter();
                // TODO Inject room data
                return true;
            }
            else
            {
                _outWriter = null;
                return false;
            }
        }

        void NetworkManager.IEventListener.Stopped(NetworkManager.StopCause _cause)
        {
            Cleanup();
            NotifyStopped(GameHostStopCause.NetworkError);
        }

        #endregion

    }

}
