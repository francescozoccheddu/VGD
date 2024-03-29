﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Offense;
using Wheeled.UI.HUD;
using Wheeled.Core;

namespace Wheeled.Networking.Client
{
    public sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager, IGameManager, OffenseBackstage.IValidationTarget
    {
        private abstract class ClientPlayer : Player
        {

            private bool m_isQuit;

            protected ClientPlayer(int _id, OffenseBackstage _offenseBackstage, bool _isLocal) : base(_id, _offenseBackstage, _isLocal)
            {
            }

            public void NotifyQuit()
            {
                if (!m_isQuit)
                {
                    m_isQuit = true;
                    EventBoardBehaviour.Instance.Put(GameManager.Current.Time, new EventBoardBehaviour.QuitEvent
                    {
                        player = this
                    });
                }
            }

            protected override void OnUpdated()
            {
                base.OnUpdated();
                if (!m_isQuit && IsQuit(GameManager.Current.Time))
                {
                    NotifyQuit();
                }
            }
        }

        double IGameManager.Time => m_time;

        private IEnumerable<NetPlayer> m_NetPlayers => m_players.Values.Where(_p => _p != m_localPlayer).Cast<NetPlayer>();

        GameRoomInfo IGameManager.Room => m_room;

        IEnumerable<IReadOnlyPlayer> IGameManager.Players => m_players.Values;

        IReadOnlyPlayer IGameManager.LocalPlayer => m_localPlayer;

        public const double c_netOffset = 0.025;

        private const double c_localOffset = 0.025;
        private const float c_timeSmoothQuickness = 0.2f;
        private const double c_quitForgetDelay = 5.0;
        private readonly OffenseBackstage m_offenseBackstage;
        private readonly OffenseBackstage m_localOffenseBackstage;
        private readonly LocalPlayer m_localPlayer;
        private readonly Dictionary<int, ClientPlayer> m_players;
        private readonly Client.IServer m_server;
        private readonly Updatable m_updatable;
        private bool m_isRunning;
        private double m_targetTime;
        private double m_time;

        private readonly GameRoomInfo m_room;

        public ClientGameManager(Client.IServer _server, int _id, GameRoomInfo _roomInfo)
        {
            GameManager.SetCurrentGameManager(this);
            m_room = _roomInfo;
            Debug.Log("ClientGameManager started");
            m_offenseBackstage = new OffenseBackstage
            {
                ValidationTarget = this
            };
            m_localOffenseBackstage = new OffenseBackstage
            {
                ValidationTarget = this
            };
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_server = _server;
            m_localPlayer = new LocalPlayer(this, _id, m_localOffenseBackstage)
            {
                HistoryDuration = 2.0,
                MaxMovementInputStepsNotifyCount = 20,
                MaxMovementNotifyFrequency = 15,
                Info = PlayerPreferences.Info
            };
            m_players = new Dictionary<int, ClientPlayer>
            {
                { _id, m_localPlayer }
            };
            // Ready notify
            Serializer.WriteReady();
            m_server.Send(NetworkManager.ESendMethod.ReliableUnordered);
        }

        void Updatable.ITarget.Update()
        {
            m_localPlayer.TimeOffset = TimeConstants.Smooth(m_localPlayer.TimeOffset, m_server.Ping + 1.0 / m_localPlayer.MaxMovementNotifyFrequency + c_localOffset, Time.deltaTime, c_timeSmoothQuickness);
            if (m_isRunning)
            {
                m_time += Time.deltaTime;
                m_targetTime += Time.deltaTime;
                m_time = TimeConstants.Smooth(m_time, m_targetTime, Time.deltaTime, c_timeSmoothQuickness);
                foreach (NetPlayer p in m_NetPlayers)
                {
                    p.TimeOffset = TimeConstants.Smooth(p.TimeOffset, -(m_server.Ping + p.AverageReplicationInterval + c_netOffset), Time.deltaTime, c_timeSmoothQuickness);
                }
                m_offenseBackstage.UpdateUntil(m_time);
                m_localOffenseBackstage.UpdateUntil(m_localPlayer.LocalTime);
                foreach (Player p in m_players.Values)
                {
                    p.Update();
                }
                double forgetTime = m_time - c_quitForgetDelay;
                foreach (KeyValuePair<int, ClientPlayer> p in (from p in m_players where p.Value.IsQuit(forgetTime) select p).ToList())
                {
                    p.Value.NotifyQuit();
                    p.Value.Destroy();
                    m_players.Remove(p.Key);
                }
            }
        }

        void Client.IGameManager.LatencyUpdated(double _latency)
        {
        }

        void Client.IGameManager.Stopped()
        {
            m_isRunning = false;
            m_updatable.IsRunning = false;
        }

        IEnumerable<OffenseBackstage.HitTarget> OffenseBackstage.IValidationTarget.ProvideHitTarget(double _time, Offense _offense)
        {
            double realTime = _time - GetPlayerById(_offense.OffenderId)?.TimeOffset ?? 0.0;
            return from p
                   in m_players.Values
                   let time = realTime + p.TimeOffset
                   where p.Id != _offense.OffenderId && p.LifeHistory.IsAlive(time) && !p.IsQuit(time)
                   select new OffenseBackstage.HitTarget { playerId = p.Id, snapshot = p.GetSnapshot(time) };
        }

        void OffenseBackstage.IValidationTarget.Damage(double _time, int _offendedId, Offense _offense, float _damage)
        {
        }

        private ClientPlayer GetPlayerById(int _id)
        {
            if (m_players.TryGetValue(_id, out ClientPlayer player))
            {
                return player;
            }
            return null;
        }

        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense)
        {
            return GetPlayerById(_offense.OffenderId)?.IsQuit(_time) == false;
        }

        private Player GetOrCreatePlayer(int _id)
        {
            if (m_players.TryGetValue(_id, out ClientPlayer player))
            {
                return player;
            }
            else
            {
                NetPlayer newNetPlayer = new NetPlayer(_id, m_offenseBackstage)
                {
                    HistoryDuration = 2.0,
                };
                m_players.Add(_id, newNetPlayer);
                EventBoardBehaviour.Instance.Put(m_time, new EventBoardBehaviour.JoinEvent
                {
                    player = newNetPlayer
                });
                return newNetPlayer;
            }
        }

        IReadOnlyPlayer IGameManager.GetPlayerById(int _id)
        {
            return GetPlayerById(_id);
        }

   
    }
}