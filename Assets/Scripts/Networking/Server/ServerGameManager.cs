using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;
using Wheeled.Networking.Client;

namespace Wheeled.Networking.Server
{
    public sealed partial class ServerGameManager : Server.IGameManager, Updatable.ITarget, IGameManager, OffenseBackstage.IValidationTarget
    {
        double IGameManager.Time => m_time;

        private IEnumerable<NetPlayer> m_NetPlayers => m_players.Where(_p => _p != m_localPlayer).Cast<NetPlayer>();

        GameRoomInfo IGameManager.Room => m_room;

        IEnumerable<IReadOnlyPlayer> IGameManager.Players => m_players;

        IReadOnlyPlayer IGameManager.LocalPlayer => m_localPlayer;

        private readonly GameRoomInfo m_room;
        private const int c_replicationRate = 10;
        private const double c_validationDelay = 0.3;
        private const float c_timeSmoothQuickness = 0.25f;
        private const double c_recapSyncPeriod = 5.0f;
        private const double c_roomUpdatePeriod = 2.0f;
        private readonly LocalPlayer m_localPlayer;
        private readonly List<AuthoritativePlayer> m_players;
        private readonly Updatable m_updatable;
        private readonly OffenseBackstage m_offenseBackstage;
        private int m_nextPlayerId;
        private double m_time;
        private float m_timeSinceLastReplication;
        private double m_lastRecapSyncTime;

        private double m_lastTimeSyncTime;

        private const int c_maxPlayerCount = 4;

        public ServerGameManager(GameRoomInfo _roomInfo)
        {
            GameManager.SetCurrentGameManager(this);
            m_room = _roomInfo;
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
                HistoryDuration = 3.0,
                MaxMovementInputStepsReplicationCount = 10,
                Info = PlayerPreferences.Info
            };
            m_nextPlayerId = 1;
            m_players = new List<AuthoritativePlayer>
            {
                m_localPlayer
            };
            m_localPlayer.Start();
        }

        void Updatable.ITarget.Update()
        {
            m_time += Time.deltaTime;
            m_timeSinceLastReplication += Time.deltaTime;
            foreach (NetPlayer player in m_NetPlayers)
            {
                double targetOffset = -Math.Min(Math.Max(player.AverageNotifyInterval, 0.0) + player.Ping, c_validationDelay);
                player.TimeOffset = TimeConstants.Smooth(player.TimeOffset, targetOffset, Time.deltaTime, c_timeSmoothQuickness);
            }
            for (int i = m_players.Count - 1; i >= 0; i--)
            {
                m_players[i].Update();
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
                SendAll(NetworkManager.ESendMethod.Sequenced);
            }
        }

        private void SendPlayerIntroductions(NetPlayer _recipient, NetworkManager.ESendMethod _method)
        {
            foreach (AuthoritativePlayer p in m_players.Where(_p => _p != _recipient))
            {
                Serializer.WritePlayerIntroductionSync(p.Id, p.Info.Value);
                _recipient.Peer.Send(_method);
            }
        }

        void Server.IGameManager.ConnectedTo(NetworkManager.Peer _peer)
        {
            if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
            {
                // Welcome
                Serializer.WritePlayerWelcomeSync(netPlayer.Id, m_room.arena);
                netPlayer.Peer.Send(NetworkManager.ESendMethod.ReliableUnordered);
                // Introduction
                SendPlayerIntroductions(netPlayer, NetworkManager.ESendMethod.ReliableUnordered);
                Serializer.WritePlayerIntroductionSync(netPlayer.Id, netPlayer.Info.Value);
                SendAllBut(netPlayer.Peer, NetworkManager.ESendMethod.ReliableUnordered);
                // Recap
                WriteRecapSync();
                netPlayer.Peer.Send(NetworkManager.ESendMethod.Unreliable);
            }
        }

        void Server.IGameManager.DisconnectedFrom(NetworkManager.Peer _peer) => GetNetPlayerByPeer(_peer)?.PutQuit(m_time);

        void Server.IGameManager.LatencyUpdated(NetworkManager.Peer _peer, double _latency) => GetNetPlayerByPeer(_peer)?.PingValue.Put(m_time, Mathf.RoundToInt((float) (_latency * 1000.0)));

        void Server.IGameManager.ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader)
        {
            try
            {

                switch (_reader.ReadMessageType())
                {
                    case EMessage.MovementNotify:
                    {
                        if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                        {
                            _reader.ReadMovementNotify(out int step, out IEnumerable<InputStep> inputSteps, out Snapshot snapshot);
                            netPlayer.TryMove(step, inputSteps, snapshot);
                        }
                    }
                    break;

                    case EMessage.ShootNotify:
                    {
                        if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                        {
                            _reader.ReadShotNotify(out double time, out ShotInfo info);
                            netPlayer.TryShoot(time, info);
                        }
                    }
                    break;

                    case EMessage.KazeNotify:
                    {
                        if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                        {
                            _reader.ReadKazeNotify(out double time, out KazeInfo info);
                            netPlayer.TryKaze(time, info);
                        }
                    }
                    break;

                    case EMessage.ReadyNotify:
                    {
                        if (ProcessPlayerMessage(_peer, out NetPlayer netPlayer))
                        {
                            netPlayer.Start();
                            Serializer.WriteTimeSync(m_time);
                            netPlayer.Peer.Send(NetworkManager.ESendMethod.Sequenced);
                            foreach (Player p in m_players.Where(_p => _p != netPlayer && _p.IsStarted && !_p.IsQuit(m_time)))
                            {
                                Serializer.WritePlayerIntroductionSync(p.Id, p.Info.Value);
                                netPlayer.Peer.Send(NetworkManager.ESendMethod.ReliableUnordered);
                            }
                            WriteRecapSync();
                            netPlayer.Peer.Send(NetworkManager.ESendMethod.Unreliable);
                        }
                    }
                    break;
                }
            }
            catch (Deserializer.DeserializationException e)
            {
                Debug.LogException(e);
            }
        }

        bool Server.IGameManager.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
        {
            if (GetNetPlayerByPeer(_peer) != null)
            {
                return false;
            }
            try
            {
                PlayerInfo info = _reader.ReadPlayerInfo();
                NetPlayer netPlayer = new NetPlayer(this, m_nextPlayerId++, _peer, m_offenseBackstage)
                {
                    HistoryDuration = 3.0,
                    MaxMovementInputStepsReplicationCount = 20,
                    MaxValidationDelay = c_validationDelay,
                    DamageValidationDelay = c_validationDelay,
                    Info = info
                };
                EventBoardBehaviour.Instance.Put(m_time, new EventBoardBehaviour.JoinEvent
                {
                    player = netPlayer
                });
                m_players.Add(netPlayer);
                return true;
            }
            catch (Deserializer.DeserializationException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        bool Server.IGameManager.ShouldReplyToDiscoveryRequest() => m_players.Count(_p => !_p.IsQuit(m_time)) < c_maxPlayerCount;

        void Server.IGameManager.Stopped() => m_updatable.IsRunning = false;

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

        void OffenseBackstage.IValidationTarget.Damage(double _time, int _offendedId, Offense _offense, float _damage)
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

        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense) => !GetPlayerById(_offense.OffenderId)?.IsQuit(_time) == true;


        private void WriteRecapSync()
        {
            double recapTime = m_time - c_validationDelay;
            Serializer.WriteRecapSync(recapTime, from p
                                                 in m_players
                                                 where p.IsStarted && !p.IsQuit(recapTime)
                                                 select p.RecapInfo(recapTime));
        }

        private NetPlayer GetNetPlayerByPeer(NetworkManager.Peer _peer) => m_NetPlayers.FirstOrDefault(_p => _p.Peer == _peer);

        private AuthoritativePlayer GetPlayerById(int _id) => m_players.FirstOrDefault(_p => _p.Id == _id);

        private bool ProcessPlayerMessage(NetworkManager.Peer _peer, out NetPlayer _outNetPlayer)
        {
            _outNetPlayer = GetNetPlayerByPeer(_peer);
            if (_outNetPlayer == null || _outNetPlayer.IsQuit(m_time))
            {
                _peer.Disconnect();
            }
            return _outNetPlayer != null;
        }

        private void SendAll(NetworkManager.ESendMethod _method)
        {
            foreach (NetPlayer netPlayer in m_NetPlayers)
            {
                netPlayer.Peer.Send(_method);
            }
        }

        private void SendAllBut(NetworkManager.Peer _peer, NetworkManager.ESendMethod _method)
        {
            foreach (NetPlayer netPlayer in m_NetPlayers.Where(_p => _p.Peer != _peer))
            {
                netPlayer.Peer.Send(_method);
            }
        }

        private void SendRoomSync()
        {
            Serializer.WriteTimeSync(m_time);
            SendAll(NetworkManager.ESendMethod.Sequenced);
        }

        IReadOnlyPlayer IGameManager.GetPlayerById(int _id) => GetPlayerById(_id);

    }
}