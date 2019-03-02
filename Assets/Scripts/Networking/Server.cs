using LiteNetLib;
using LiteNetLib.Utils;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed class Server : NetworkHost
    {

        public Server(NetworkInstance _netInstance) : base(_netInstance)
        {
        }

        public override void ConnectedTo(NetworkManager.IPeer _peer)
        {
            throw new System.NotImplementedException();
        }

        public override void DisconnectedFrom(NetworkManager.IPeer _peer)
        {
            throw new System.NotImplementedException();
        }

        public override void Moved(NetPeer _key)
        {
            throw new System.NotImplementedException();
        }

        public override void ReceivedFrom(NetworkManager.IPeer _peer, NetPacketReader _reader)
        {
            throw new System.NotImplementedException();
        }

        public override bool ShouldAcceptConnectionRequest(NetDataReader _reader)
        {
            throw new System.NotImplementedException();
        }

        public override bool ShouldReplyToDiscoveryRequest()
        {
            throw new System.NotImplementedException();
        }

    }

}
