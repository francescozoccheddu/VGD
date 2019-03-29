using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager : Server.IGameManager, Updatable.ITarget
    {

        private const double c_respawnWaitTime = 2.0;
        private const double c_spawnDelay = 0.5;

        private const int c_replicationRate = 10;
        private const bool c_sendInputReplication = true;
        private const int c_maxInputSteps = 20;

        private readonly Updatable m_updatable;
        private readonly List<NetPlayer> m_netPlayers;
        private byte m_nextPlayerId;
        private float m_timeSinceLastReplication;

        private double m_time;

        public ServerGameManager()
        {
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_time = 0.0;
            // Net players
            m_netPlayers = new List<NetPlayer>();
            m_nextPlayerId = 1;
            // Local player
            m_movementController = new MovementController();
            m_inputHistory = new InputHistory();
            m_actionHistory = new ActionHistory();
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

        private readonly InputStep[] m_inputStepBuffer = new InputStep[c_maxInputSteps];

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader)
        {
            // TODO Catch exception
            switch (_reader.ReadMessageType())
            {
                case Message.MovementNotify:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        _reader.ReadMovementNotifyMessage(out int step, out int inputStepCount, m_inputStepBuffer, out Snapshot snapshot);
                        netPlayer.Move(step, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepCount), snapshot);
                    }
                }
                break;
                case Message.ReadyNotify:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        netPlayer.Start();
                        PrepareRoomSyncMessage(netPlayer);
                        _peer.Send(NetworkManager.SendMethod.Sequenced);
                        IEnumerable<PlayerSyncInfo> GetInfos()
                        {
                            yield return GetLocalPlayerSyncInfo();
                            foreach (NetPlayer p in m_netPlayers)
                            {
                                if (p != netPlayer)
                                {
                                    yield return GetPlayerSyncInfo(p);
                                }
                            }
                        }
                        Serializer.WritePlayerSync(m_time, m_netPlayers.Count, GetInfos());
                        _peer.Send(NetworkManager.SendMethod.Sequenced);
                    }
                }
                break;
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
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
            m_updatable.IsRunning = false;
        }

        #endregion

        #region Room Update

        private const double c_roomUpdatePeriod = 2.0f;
        private double m_lastRoomUpdateTime;

        private void PrepareRoomSyncMessage(NetPlayer _player)
        {
            Serializer.WriteRoomSyncMessage(m_time, _player.stats, (byte) _player.GetHealth());
        }

        private PlayerSyncInfo GetLocalPlayerSyncInfo()
        {
            return new PlayerSyncInfo
            {
                id = 0,
                kills = 0,
                deaths = 0,
                health = (byte) m_actionHistory.Health,
                ping = 0,
                info = new PlayerInfo()
            };
        }

        private PlayerSyncInfo GetPlayerSyncInfo(NetPlayer _netPlayer)
        {
            return new PlayerSyncInfo
            {
                id = _netPlayer.id,
                kills = 0,
                deaths = 0,
                health = (byte) _netPlayer.GetHealth(),
                ping = (byte) Mathf.Min(Mathf.RoundToInt(_netPlayer.peer.Ping * 1000.0f), 255),
                info = _netPlayer.info
            };
        }

        private void SendRoomSync()
        {
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                PrepareRoomSyncMessage(netPlayer);
                netPlayer.peer.Send(NetworkManager.SendMethod.Sequenced);
                IEnumerable<PlayerSyncInfo> GetInfos()
                {
                    yield return GetLocalPlayerSyncInfo();
                    foreach (NetPlayer p in m_netPlayers)
                    {
                        if (p != netPlayer)
                        {
                            yield return GetPlayerSyncInfo(p);
                        }
                    }
                }
                Serializer.WritePlayerSync(m_time, m_netPlayers.Count, GetInfos());
                netPlayer.peer.Send(NetworkManager.SendMethod.Sequenced);
            }
        }

        #endregion

        void Updatable.ITarget.Update()
        {
            m_time += Time.deltaTime;
            m_timeSinceLastReplication += Time.deltaTime;
            if (m_lastRoomUpdateTime + c_roomUpdatePeriod <= m_time)
            {
                Debug.LogFormat("Room Update at time {0}", m_time);
                m_lastRoomUpdateTime = m_time;
                SendRoomSync();
            }

            UpdateLocalPlayer();
            foreach (NetPlayer player in m_netPlayers)
            {
                player.Update();
            }
            if (m_timeSinceLastReplication > 1.0f / c_replicationRate)
            {
                m_timeSinceLastReplication = 0.0f;
                ReplicateLocalPlayer(false, true);
                foreach (NetPlayer player in m_netPlayers)
                {
                    player.SendReplication(true, false);
                }
            }
        }

        private void SendAll(NetworkManager.SendMethod _method)
        {
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                netPlayer.peer.Send(_method);
            }
        }

        private void SendAllBut(NetworkManager.Peer _peer, NetworkManager.SendMethod _method)
        {
            foreach (NetPlayer netPlayer in m_netPlayers)
            {
                if (netPlayer.peer != _peer)
                {
                    netPlayer.peer.Send(_method);
                }
            }
        }

    }

}
