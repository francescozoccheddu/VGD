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

        #region Server.IGameManager

        void Server.IGameManager.ConnectedTo(NetworkManager.Peer _peer)
        {
        }

        void Server.IGameManager.DisconnectedFrom(NetworkManager.Peer _peer)
        {
        }

        void Server.IGameManager.LatencyUpdated(NetworkManager.Peer _peer, int _latency)
        {
        }

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            return true;
        }

        bool Server.IGameManager.ShouldReplyToDiscoveryRequest()
        {
            return true;
        }

        void Server.IGameManager.Stopped()
        {
        }

        #endregion

    }

}
