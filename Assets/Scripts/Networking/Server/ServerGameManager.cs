using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking.Server
{

    internal sealed class ServerGameManager : Server.IGameManager, IUpdatable
    {

        private readonly struct NetPlayer
        {

            public readonly int id;
            public readonly PlayerHolders.AuthoritativePlayerHolder player;
            public readonly NetworkManager.Peer peer;

            public NetPlayer(int _id, PlayerHolders.AuthoritativePlayerHolder _player, NetworkManager.Peer _peer)
            {
                id = _id;
                player = _player;
                peer = _peer;
            }

        }

        private readonly UpdatableHolder m_roomUpdateHolder;
        private readonly List<NetPlayer> m_netPlayers;
        private int m_nextPlayerId;

        public ServerGameManager()
        {
            m_roomUpdateHolder = new UpdatableHolder(this, false)
            {
                IsRunning = true
            };
            RoomTime.Manager.Set(TimeStep.zero, false);
            RoomTime.Manager.Start();
            m_netPlayers = new List<NetPlayer>();
            m_nextPlayerId = 0;
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
            // TODO decide weather accept it or not
            m_netPlayers.Add(new NetPlayer(m_nextPlayerId++, PlayerHolders.NewAuthoritativePlayer(), _peer));
            return true;
        }

        bool Server.IGameManager.ShouldReplyToDiscoveryRequest()
        {
            return true;
        }

        void Server.IGameManager.Stopped()
        {
            RoomTime.Manager.Stop();
            m_roomUpdateHolder.IsRunning = false;
        }

        #endregion

        #region Room Update

        private const float c_roomUpdatePeriod = 2.0f;
        private TimeStep m_lastRoomUpdateTime;

        private void RoomUpdate()
        {
            Serializer.WriteRoomUpdateMessage(RoomTime.Now);
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                netPlayer.peer.Send(Serializer.writer, DeliveryMethod.Unreliable);
            }
        }

        void IUpdatable.Update()
        {
            if ((m_lastRoomUpdateTime + c_roomUpdatePeriod) <= RoomTime.Now)
            {
                Debug.LogFormat("Room Update at time {0}", RoomTime.Now);
                m_lastRoomUpdateTime = RoomTime.Now;
                RoomUpdate();
            }
        }

        #endregion

    }

}
