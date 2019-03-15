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
        private byte m_nextPlayerId;

        public ServerGameManager()
        {
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            RoomTime.Manager.Set(TimeStep.zero, false);
            RoomTime.Manager.Start();
            m_netPlayers = new List<NetPlayer>();
            m_nextPlayerId = 1;


            m_movementController = new MovementController(3.0f);
            m_view = new PlayerView();
            StartLocalPlayer();
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

        private bool ProcessPlayerMessage(NetworkManager.Peer _peer, out NetPlayer _outNetPlayer)
        {
            _outNetPlayer = GetNetPlayerByPeer(_peer);
            if (_outNetPlayer == null)
            {
                _peer.Disconnect();
            }
            return _outNetPlayer != null;
        }

        #region Server.IGameManager

        void Server.IGameManager.ConnectedTo(NetworkManager.Peer _peer)
        {
        }

        void Server.IGameManager.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            for (int i = 0; i < m_netPlayers.Count; i++)
            {
                if (m_netPlayers[i].peer == _peer)
                {
                    m_netPlayers[i].Destroy();
                    m_netPlayers.RemoveAt(i);
                    return;
                }
            }
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
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        _reader.ReadSimulationMessage(out int firstStep, m_inputStepBuffer, out int inputStepsCount, out SimulationStep simulation);
                        netPlayer.Move(firstStep, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepsCount), simulation);
                    }
                }
                break;
                case Message.Sight:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        _reader.ReadSightMessage(out int step, out Sight sight);
                        netPlayer.Sight(step, sight);
                    }
                }
                break;
                case Message.Ready:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        netPlayer.Start();
                        PrepareRoomUpdateMessage();
                        _peer.Send(Serializer.writer, DeliveryMethod.Unreliable);
                    }
                }
                break;
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, NetDataReader _reader)
        {
            // TODO decide whether accept it or not
            NetPlayer netPlayer = new NetPlayer(this, m_nextPlayerId++, _peer);
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

        private void PrepareRoomUpdateMessage()
        {
            Serializer.WriteRoomUpdateMessage(RoomTime.Now);
        }

        private void RoomUpdate()
        {
            PrepareRoomUpdateMessage();
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                netPlayer.peer.Send(Serializer.writer, DeliveryMethod.Unreliable);
            }
        }

        #endregion

        void Updatable.ITarget.Update()
        {
            RoomTime.Manager.Update();
            if ((m_lastRoomUpdateTime + c_roomUpdatePeriod) <= RoomTime.Now)
            {
                Debug.LogFormat("Room Update at time {0}", RoomTime.Now);
                m_lastRoomUpdateTime = RoomTime.Now;
                RoomUpdate();
            }
            UpdateLocalPlayer();
            foreach (NetPlayer player in m_netPlayers)
            {
                player.Update();
            }
        }

        private void SendAll(DeliveryMethod _deliveryMethod)
        {
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                netPlayer.peer.Send(Serializer.writer, _deliveryMethod);
            }
        }

        private void SendAllBut(NetworkManager.Peer _peer, DeliveryMethod _deliveryMethod)
        {
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                if (netPlayer.peer != _peer)
                {
                    netPlayer.peer.Send(Serializer.writer, _deliveryMethod);
                }
            }
        }

    }

}
