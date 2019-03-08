using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Wheeled.Networking
{

    internal sealed class ServerGameManager : Server.IGameManager
    {

        public ServerGameManager()
        {
            Debug.Log("ServerGameManager constructed");
        }

        public void ConnectedTo(NetworkManager.Peer _peer)
        {
        }

        public void DisconnectedFrom(NetworkManager.Peer _peer)
        {
        }

        public void LatencyUpdated(NetworkManager.Peer _peer, int _latency)
        {
        }

        public void ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
        }

        public bool ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            return true;
        }

        public bool ShouldReplyToDiscoveryRequest()
        {
            return true;
        }

        public void Stopped()
        {
        }

    }

}
