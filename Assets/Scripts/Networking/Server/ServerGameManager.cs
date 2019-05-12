using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Stage;
using Wheeled.HUD;
using Wheeled.Networking.Client;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager : Server.IGameManager, Updatable.ITarget, IPlayerManager, OffenseBackstage.IValidationTarget
    {
        #region Public Properties

        double IPlayerManager.Time => m_time;

        public EventBoardDispatcher MatchBoard { get; }

        #endregion Public Properties

        #region Private Properties

        private IEnumerable<NetPlayer> m_NetPlayers => m_players.Where(_p => _p != m_localPlayer).Cast<NetPlayer>();

        #endregion Private Properties

        #region Private Fields

        private const int c_replicationRate = 10;
        private const double c_validationDelay = 0.3;
        private const float c_timeSmoothQuickness = 0.25f;
        private const double c_recapSyncPeriod = 5.0f;
        private const double c_roomUpdatePeriod = 2.0f;
        private readonly LocalPlayer m_localPlayer;
        private readonly List<AuthoritativePlayer> m_players;
        private readonly Updatable m_updatable;
        private readonly OffenseBackstage m_offenseBackstage;
        private byte m_nextPlayerId;
        private double m_time;
        private float m_timeSinceLastReplication;
        private double m_lastRecapSyncTime;

        private double m_lastTimeSyncTime;

        private readonly byte m_arena;

        #endregion Private Fields

        #region Public Constructors

        public ServerGameManager(byte _arena)
        {
            m_offenseBackstage = new OffenseBackstage
            {
                ValidationTarget = this
            };
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_time = 0.0;
            m_localPlayer = new LocalPlayer(this, 0, m_offenseBackstage)
            {
                HistoryDuration = 2.0,
                MaxMovementInputStepsReplicationCount = 5,
                Info = PlayerPreferences.Info
            };
            m_nextPlayerId = 1;
            m_players = new List<AuthoritativePlayer>
            {
                m_localPlayer
            };
            MatchBoard = new EventBoardDispatcher();
            m_arena = _arena;
            m_localPlayer.Start();
        }

        #endregion Public Constructors

        #region Public Methods

        void Updatable.ITarget.Update()
        {
            m_time += Time.deltaTime;
            m_timeSinceLastReplication += Time.deltaTime;
            foreach (NetPlayer player in m_NetPlayers)
            {
                double targetOffset = -Math.Min(Math.Max(player.AverageNotifyInterval, 0.0) + player.Ping, c_validationDelay);
                player.TimeOffset = TimeConstants.Smooth(player.TimeOffset, targetOffset, Time.deltaTime, c_timeSmoothQuickness);
            }
            foreach (AuthoritativePlayer player in m_players)
            {
                player.Update();
            }
            m_offenseBackstage.UpdateUntil(m_time);
            if (m_lastTimeSyncTime + c_roomUpdatePeriod <= m_time)
            {
                m_lastTimeSyncTime = m_time;
                SendRoomSync();
            }
            if (m_timeSinceLastReplication > 1.0f / c_replicationRate)
            {
                m_timeSinceLastReplication = 0.0f;
                foreach (AuthoritativePlayer player in m_players)
                {
                    player.Replicate();
                }
            }
            if (m_lastRecapSyncTime + c_recapSyncPeriod <= m_time)
            {
                m_lastRecapSyncTime = m_time;
                WriteRecapSync();
                SendAll(NetworkManager.SendMethod.Sequenced);
            }
            MatchBoard.UpdateUntil(m_time);
        }

        void Server.IGameManager.ConnectedTo(NetworkManager.Peer _peer)
        {
            if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
            {
                // Welcome
                Serializer.WritePlayerWelcomeSync(netPlayer.Id, m_arena);
                netPlayer.Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
                // Introduction (so that he knows the others)
                foreach (AuthoritativePlayer p in m_players.Where(_p => _p != netPlayer))
                {
                    Serializer.WritePlayerIntroductionSync(p.Id, p.Info.Value);
                    netPlayer.Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
                }
                // Introduction (so that the others know him)
                Serializer.WritePlayerIntroductionSync(netPlayer.Id, netPlayer.Info.Value);
                SendAllBut(netPlayer.Peer, NetworkManager.SendMethod.ReliableUnordered);
                // Recap
                WriteRecapSync();
                netPlayer.Peer.Send(NetworkManager.SendMethod.Unreliable);
            }
        }

        void Server.IGameManager.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            GetNetPlayerByPeer(_peer)?.PutQuit(m_time);
        }

        void Server.IGameManager.LatencyUpdated(NetworkManager.Peer _peer, double _latency)
        {
            GetNetPlayerByPeer(_peer)?.PingValue.Put(m_time, Mathf.RoundToInt((float) (_latency * 1000.0)));
            UpdateScoreBoard();
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
                        _reader.ReadKazeNotify(out double time, out KazeInfo info);
                        netPlayer.TryKaze(time, info);
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
                        foreach (Player p in m_players.Where(_p => _p != netPlayer && _p.IsStarted && !_p.IsQuit(m_time)))
                        {
                            Serializer.WritePlayerIntroductionSync(p.Id, p.Info.Value);
                            netPlayer.Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
                        }
                        WriteRecapSync();
                        netPlayer.Peer.Send(NetworkManager.SendMethod.Unreliable);
                    }
                }
                break;
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
        {
            if (GetNetPlayerByPeer(_peer) != null)
            {
                return false;
            }
            NetPlayer netPlayer = new NetPlayer(this, m_nextPlayerId++, _peer, m_offenseBackstage)
            {
                HistoryDuration = 3.0,
                MaxMovementInputStepsReplicationCount = 20,
                MaxValidationDelay = c_validationDelay,
                DamageValidationDelay = c_validationDelay,
                Info = _reader.ReadPlayerInfo()
            };
            MatchBoard.Put(m_time, new EventBoardDispatcher.JoinEvent
            {
                player = netPlayer
            });
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

        IEnumerable<OffenseBackstage.HitTarget> OffenseBackstage.IValidationTarget.ProvideHitTarget(double _time, Offense _offense)
        {
            double realTime = _time - GetPlayerById(_offense.OffenderId)?.TimeOffset ?? 0.0;
            double offenderDelay = (GetPlayerById(_offense.OffenderId) as NetPlayer)?.Peer.Ping + 1.0 / c_replicationRate + ClientGameManager.c_netOffset ?? 0.0;
            return from p
                   in m_players
                   let time = realTime - (_offense.OffenderId == m_localPlayer.Id ? -p.TimeOffset : offenderDelay)
                   where p.Id != _offense.OffenderId && p.LifeHistory.IsAlive(time) && !p.IsQuit(time)
                   select new OffenseBackstage.HitTarget { playerId = p.Id, snapshot = p.GetSnapshot(time) };
        }

        void OffenseBackstage.IValidationTarget.Damage(double _time, byte _offendedId, Offense _offense, float _damage)
        {
            AuthoritativePlayer offended = GetPlayerById(_offendedId);
            int damage = Mathf.RoundToInt(_damage * LifeHistory.c_fullHealth);
            offended?.PutDamage(_time, new DamageInfo
            {
                damage = damage,
                maxHealth = offended.LifeHistory.GetHealth(_time) - damage,
                offenderId = _offense.OffenderId,
                offenseType = _offense.Type
            });
        }

        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense)
        {
            return !GetPlayerById(_offense.OffenderId)?.IsQuit(_time) == true;
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateScoreBoard()
        {
            IEnumerable<AuthoritativePlayer> players = from p
                                                       in m_players
                                                       where p.IsStarted && !p.IsQuit(m_time)
                                                       select p;
            ScoreBoardBehaviour.Update(players);
        }

        private void WriteRecapSync()
        {
            double recapTime = m_time - c_validationDelay;
            Serializer.WriteRecapSync(recapTime, from p
                                                 in m_players
                                                 where p.IsStarted && !p.IsQuit(recapTime)
                                                 select p.RecapInfo(recapTime));
        }

        private NetPlayer GetNetPlayerByPeer(NetworkManager.Peer _peer)
        {
            return m_NetPlayers.FirstOrDefault(_p => _p.Peer == _peer);
        }

        private AuthoritativePlayer GetPlayerById(int _id)
        {
            return m_players.FirstOrDefault(_p => _p.Id == _id);
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

        private void SendRoomSync()
        {
            Serializer.WriteTimeSync(m_time);
            SendAll(NetworkManager.SendMethod.Sequenced);
        }

        #endregion Private Methods
    }
}