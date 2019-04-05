using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager : Server.IGameManager, Updatable.ITarget, IGameManager
    {
        private const int c_replicationRate = 10;
        private readonly LocalPlayer m_localPlayer;
        private readonly List<Player> m_players;
        private readonly ShootStage m_shootStage;
        private readonly Updatable m_updatable;
        private byte m_nextPlayerId;
        private double m_time;
        private float m_timeSinceLastReplication;

        public ServerGameManager()
        {
            m_shootStage = new ShootStage();
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_time = 0.0;
            m_localPlayer = new LocalPlayer(this, 0)
            {
                HistoryDuration = 2.0,
                MaxMovementInputStepsReplicationCount = 5,
                SpawnDelay = 0.5
            };
            m_localPlayer.Introduce(new PlayerInfo());
            m_nextPlayerId = 1;
            m_players = new List<Player>
            {
                m_localPlayer
            };
            m_localPlayer.Start();
        }

        ShootStage IGameManager.ShootStage => m_shootStage;
        double IGameManager.Time => m_time;
        private IEnumerable<NetPlayer> m_NetPlayers => m_players.Where(_p => _p != m_localPlayer).Cast<NetPlayer>();

        void Updatable.ITarget.Update()
        {
            m_time += Time.deltaTime;
            m_timeSinceLastReplication += Time.deltaTime;
            foreach (Player player in m_players)
            {
                player.Update();
            }
            m_shootStage.Update(m_time);
            if (m_lastTimeSyncTime + c_roomUpdatePeriod <= m_time)
            {
                m_lastTimeSyncTime = m_time;
                SendRoomSync();
            }
            if (m_timeSinceLastReplication > 1.0f / c_replicationRate)
            {
                m_timeSinceLastReplication = 0.0f;
                foreach (Player player in m_players)
                {
                    player.Replicate();
                }
            }
            if (m_lastRecapSyncTime + c_recapSyncPeriod <= m_time)
            {
                m_lastRecapSyncTime = m_time;
                Serializer.WriteRecapSync(m_time, from p in m_players select p.RecapInfo);
                SendAll(NetworkManager.SendMethod.Sequenced);
            }
        }

        private NetPlayer GetNetPlayerByPeer(NetworkManager.Peer _peer)
        {
            return m_NetPlayers.FirstOrDefault(_p => _p.Peer == _peer);
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

        private void SendAll(NetworkManager.SendMethod _method)
        {
            foreach (NetPlayer netPlayer in m_NetPlayers)
            {
                netPlayer.Peer.Send(_method);
            }
        }

        private void SendAllBut(NetworkManager.Peer _peer, NetworkManager.SendMethod _method)
        {
            foreach (NetPlayer netPlayer in m_NetPlayers.Where(_p => _p.Peer != _peer))
            {
                netPlayer.Peer.Send(_method);
            }
        }

        #region Server.IGameManager

        void Server.IGameManager.ConnectedTo(NetworkManager.Peer _peer)
        {
            if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
            {
                // Welcome
                Serializer.WritePlayerWelcomeSync(netPlayer.Id);
                netPlayer.Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
                // Introduction (so that he knows the others)
                foreach (Player p in m_players.Where(_p => _p != netPlayer))
                {
                    Serializer.WritePlayerIntroductionSync(p.Id, p.Info.Value);
                    netPlayer.Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
                }
                // Introduction (so that the others know him)
                Serializer.WritePlayerIntroductionSync(netPlayer.Id, netPlayer.Info.Value);
                SendAllBut(netPlayer.Peer, NetworkManager.SendMethod.ReliableUnordered);
                // Recap
                Serializer.WriteRecapSync(m_time, from p in m_players select p.RecapInfo);
                netPlayer.Peer.Send(NetworkManager.SendMethod.Unreliable);
            }
        }

        void Server.IGameManager.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            GetNetPlayerByPeer(_peer)?.PutQuit(m_time);
        }

        void Server.IGameManager.LatencyUpdated(NetworkManager.Peer _peer, double _latency)
        {
        }

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader)
        {
            // TODO Catch exception
            switch (_reader.ReadMessageType())
            {
                case Message.MovementNotify:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        _reader.ReadMovementNotify(out int step, out IEnumerable<InputStep> inputSteps, out Snapshot snapshot);
                        netPlayer.TryMove(step, inputSteps, snapshot);
                    }
                }
                break;

                case Message.ShootNotify:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        _reader.ReadShotNotify(out double time, out ShotInfo info);
                        netPlayer.TryShoot(time, info);
                    }
                }
                break;

                case Message.KazeNotify:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        _reader.ReadKazeNotify(out double time);
                        netPlayer.TryKaze(time);
                    }
                }
                break;

                case Message.ReadyNotify:
                {
                    if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                    {
                        netPlayer.Start();
                        Serializer.WriteTimeSync(m_time);
                        netPlayer.Peer.Send(NetworkManager.SendMethod.Sequenced);
                        foreach (NetPlayer p in m_NetPlayers)
                        {
                            Serializer.WritePlayerIntroductionSync(p.Id, p.Info.Value);
                            netPlayer.Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
                        }
                        Serializer.WriteRecapSync(m_time, from p in m_players select p.RecapInfo);
                        netPlayer.Peer.Send(NetworkManager.SendMethod.Unreliable);
                    }
                }
                break;
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
        {
            // TODO decide whether accept it or not
            NetPlayer netPlayer = new NetPlayer(this, m_nextPlayerId++, _peer)
            {
                HistoryDuration = 2.0,
                MaxMovementInputStepsReplicationCount = 5,
                SpawnDelay = 0.5,
            };
            netPlayer.Introduce(new PlayerInfo());
            m_players.Add(netPlayer);
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

        #endregion Server.IGameManager

        #region Room Update

        private const double c_recapSyncPeriod = 5.0f;
        private const double c_roomUpdatePeriod = 2.0f;
        private double m_lastRecapSyncTime;
        private double m_lastTimeSyncTime;

        private void SendRoomSync()
        {
            Serializer.WriteTimeSync(m_time);
            SendAll(NetworkManager.SendMethod.Sequenced);
        }

        #endregion Room Update
    }
}