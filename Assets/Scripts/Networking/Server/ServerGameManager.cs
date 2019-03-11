using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
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

        private InputStep[] steps = new InputStep[8];

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
            _reader.ReadInteractivePlayerData(out int _firstStep, steps, out Snapshot snapshot);
            Debug.LogFormat("[{0}] Moved to {1}", _firstStep, snapshot.simulation.position);
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
