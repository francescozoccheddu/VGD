using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

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

        private NetPlayer? GetNetPlayerByPeer(NetworkManager.Peer _peer)
        {
            foreach (NetPlayer player in m_netPlayers)
            {
                if (player.peer == _peer)
                {
                    return player;
                }
            }
            return null;
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

        private readonly InputStep[] m_inputStepBuffer = new InputStep[12];

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
            switch (_reader.ReadMessageType())
            {
                case Message.Movement:
                {
                    NetPlayer? netPlayer = GetNetPlayerByPeer(_peer);
                    if (netPlayer != null)
                    {
                        PlayerHolders.AuthoritativePlayerHolder player = netPlayer.Value.player;
                        _reader.ReadMovementMessage(out int firstStep, m_inputStepBuffer, out int inputStepsCount, out Snapshot snapshot);
                        player.movementValidator.Put(firstStep, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepsCount), snapshot.simulation);
                    }
                }
                break;
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            // TODO decide weather accept it or not
            PlayerHolders.AuthoritativePlayerHolder player = PlayerHolders.NewAuthoritativePlayer();
            player.movementValidator.maxTrustedSteps = 10;
            player.movementValidator.StartAt(RoomTime.Now.Step, true);
            m_netPlayers.Add(new NetPlayer(m_nextPlayerId++, player, _peer));
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
