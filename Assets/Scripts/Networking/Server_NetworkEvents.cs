using LiteNetLib;
using LiteNetLib.Utils;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed partial class Server : IEventListener
    {

        private readonly NetworkInstance m_network;

        public Server(NetworkInstance _network)
        {
            m_network = _network;
        }

        public void ConnectedTo(IPeer _peer)
        {
            throw new System.NotImplementedException();
        }

        public void DisconnectedFrom(IPeer _peer)
        {
            throw new System.NotImplementedException();
        }

        public void ReceivedFrom(IPeer _peer, NetPacketReader _reader)
        {
            throw new System.NotImplementedException();
        }

        public bool ShouldAcceptConnectionRequest(NetDataReader _reader)
        {
            throw new System.NotImplementedException();
        }

        public bool ShouldReplyToDiscoveryRequest()
        {
            throw new System.NotImplementedException();
        }

    }

}
