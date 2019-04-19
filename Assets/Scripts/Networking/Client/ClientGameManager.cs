﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager, IPlayerManager, OffenseBackstage.IValidationTarget
    {
        #region Public Properties

        OffenseBackstage IPlayerManager.OffenseBackstage => m_offenseBackstage;
        double IPlayerManager.Time => m_time;

        #endregion Public Properties

        #region Private Properties

        private IEnumerable<NetPlayer> m_NetPlayers => m_players.Values.Where(_p => _p != m_localPlayer).Cast<NetPlayer>();

        #endregion Private Properties

        #region Public Fields

        public const double c_netOffset = 0.025;

        #endregion Public Fields

        #region Private Fields

        private const double c_localOffset = 0.025;
        private const float c_timeSmoothQuickness = 0.2f;
        private readonly LocalPlayer m_localPlayer;
        private readonly Dictionary<byte, PlayerBase> m_players;
        private readonly Client.IServer m_server;
        private readonly OffenseBackstage m_offenseBackstage;
        private readonly Updatable m_updatable;
        private bool m_isRunning;
        private double m_targetTime;
        private double m_time;

        #endregion Private Fields

        #region Public Constructors

        public ClientGameManager(Client.IServer _server, byte _id)
        {
            Debug.Log("ClientGameManager started");
            m_offenseBackstage = new OffenseBackstage
            {
                ValidationTarget = this
            };
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_server = _server;
            m_localPlayer = new LocalPlayer(this, _id)
            {
                HistoryDuration = 2.0,
                MaxMovementInputStepsNotifyCount = 20,
                MaxMovementNotifyFrequency = 15
            };
            m_players = new Dictionary<byte, PlayerBase>
            {
                { _id, m_localPlayer }
            };
            // Ready notify
            Serializer.WriteReady();
            m_server.Send(NetworkManager.SendMethod.ReliableUnordered);
        }

        #endregion Public Constructors

        #region Public Methods

        void Updatable.ITarget.Update()
        {
            double owd = m_server.Ping / 2.0;
            m_localPlayer.TimeOffset = TimeConstants.Smooth(m_localPlayer.TimeOffset, owd + 1.0 / m_localPlayer.MaxMovementNotifyFrequency + c_localOffset, Time.deltaTime, c_timeSmoothQuickness);
            if (m_isRunning)
            {
                m_time += Time.deltaTime;
                m_targetTime += Time.deltaTime;
                m_time = TimeConstants.Smooth(m_time, m_targetTime, Time.deltaTime, c_timeSmoothQuickness);
            }
            foreach (NetPlayer p in m_NetPlayers)
            {
                p.TimeOffset = TimeConstants.Smooth(p.TimeOffset, -(owd + p.AverageReplicationInterval + c_netOffset), Time.deltaTime, c_timeSmoothQuickness);
            }
            foreach (PlayerBase p in m_players.Values)
            {
                p.Update();
            }
            m_offenseBackstage.UpdateUntil(m_time);
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
            return from p
                  in m_players.Values
                   where p.Id != _offense.OffenderId && p.LifeHistory.IsAlive(_time) && !p.IsQuit(_time)
                   select new OffenseBackstage.HitTarget { playerId = p.Id, snapshot = p.GetSnapshot(_time) };
        }

        void OffenseBackstage.IValidationTarget.Damage(double _time, byte _offendedId, Offense _offense, float _damage)
        {
        }

        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense)
        {
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private PlayerBase GetOrCreatePlayer(byte _id)
        {
            if (m_players.TryGetValue(_id, out PlayerBase player))
            {
                return player;
            }
            else
            {
                NetPlayer newNetPlayer = new NetPlayer(this, _id)
                {
                    HistoryDuration = 2.0,
                };
                m_players.Add(_id, newNetPlayer);
                return newNetPlayer;
            }
        }

        #endregion Private Methods
    }
}