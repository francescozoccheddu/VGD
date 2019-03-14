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

    internal sealed partial class ServerGameManager : Server.IGameManager, Updatable.ITarget
    {

        private readonly Updatable m_updatable;
        private readonly List<NetPlayer> m_netPlayers;
        private int m_nextPlayerId;

        public ServerGameManager()
        {
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            RoomTime.Manager.Set(TimeStep.zero, false);
            RoomTime.Manager.Start();
            m_netPlayers = new List<NetPlayer>();
            m_nextPlayerId = 0;
        }

        private NetPlayer GetNetPlayerByPeer(NetworkManager.Peer _peer)
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

        void Server.IGameManager.LatencyUpdated(NetworkManager.Peer _peer, float _latency)
        {
        }

        private readonly InputStep[] m_inputStepBuffer = new InputStep[12];

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, NetPacketReader _reader)
        {
            switch (_reader.ReadMessageType())
            {
                case Message.Simulation:
                {
                    NetPlayer netPlayer = GetNetPlayerByPeer(_peer);
                    if (netPlayer != null)
                    {
                        _reader.ReadSimulationMessage(out int firstStep, m_inputStepBuffer, out int inputStepsCount, out SimulationStep simulation);
                        netPlayer.Move(firstStep, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepsCount), simulation);
                    }
                }
                break;
                case Message.Sight:
                {
                    NetPlayer netPlayer = GetNetPlayerByPeer(_peer);
                    if (netPlayer != null)
                    {
                        _reader.ReadSightMessage(out int step, out Sight sight);
                        netPlayer.Sight(step, sight);
                    }
                }
                break;
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            // TODO decide weather accept it or not
            NetPlayer netPlayer = new NetPlayer(this, m_nextPlayerId++, _peer);
            netPlayer.Start();
            m_netPlayers.Add(netPlayer);
            return true;
        }

        bool Server.IGameManager.ShouldReplyToDiscoveryRequest()
        {
            return true;
        }

        void Server.IGameManager.Stopped()
        {
            RoomTime.Manager.Stop();
            m_updatable.IsRunning = false;
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

        void Updatable.ITarget.Update()
        {
            RoomTime.Manager.Update();
            if ((m_lastRoomUpdateTime + c_roomUpdatePeriod) <= RoomTime.Now)
            {
                Debug.LogFormat("Room Update at time {0}", RoomTime.Now);
                m_lastRoomUpdateTime = RoomTime.Now;
                RoomUpdate();
            }
            foreach (NetPlayer player in m_netPlayers)
            {
                player.Update();
            }
        }

        #endregion

    }

}
