using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Debugging;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager, IGameManager, OffenseStage.IValidationTarget
    {
        private const int c_expectedMovementReplicationFrequency = 10;
        private const double c_localOffset = 0.5;
        private const double c_netOffset = -0.25;
        private const double c_timeSmoothQuickness = 0.5;
        private readonly LocalPlayer m_localPlayer;
        private readonly Dictionary<byte, Player> m_players;
        private readonly Client.IServer m_server;
        private readonly OffenseStage m_shootStage;
        private readonly Updatable m_updatable;
        private bool m_isRunning;
        private double m_targetTime;
        private double m_time;

        public ClientGameManager(Client.IServer _server, byte _id)
        {
            Debug.Log("ClientGameManager started");
            m_shootStage = new OffenseStage
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
                MaxMovementNotifyFrequency = 10
            };
            m_players = new Dictionary<byte, Player>
            {
                { _id, m_localPlayer }
            };
            // Ready notify
            Serializer.WriteReady();
            m_server.Send(NetworkManager.SendMethod.ReliableUnordered);
        }

        OffenseStage IGameManager.ShootStage => m_shootStage;

        double IGameManager.Time => m_time;

        void Updatable.ITarget.Update()
        {
            double owd = m_server.Ping / 2.0;
            m_localPlayer.TimeOffset = LerpTime(m_localPlayer.TimeOffset, owd + 1.0 / m_localPlayer.MaxMovementNotifyFrequency + c_localOffset, Time.deltaTime);
            if (m_isRunning)
            {
                m_time += Time.deltaTime;
                m_targetTime += Time.deltaTime;
                m_time = LerpTime(m_time, m_targetTime, Time.deltaTime);
                Printer.Print("Offset", m_targetTime - m_time);
            }
            foreach (Player p in m_players.Values)
            {
                p.TimeOffset = LerpTime(p.TimeOffset, -(owd + 1.0 / c_expectedMovementReplicationFrequency) + c_netOffset, Time.deltaTime);
                p.Update();
            }
            m_shootStage.Update(m_time);
        }

        private static double LerpTime(double _a, double _b, double _deltaTime)
        {
            double alpha = Math.Max(_deltaTime * c_timeSmoothQuickness, 0.5);
            return _a * (1 - alpha) + _b * alpha;
        }

        private Player GetOrCreatePlayer(byte _id)
        {
            if (m_players.TryGetValue(_id, out Player player))
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

        #region Client.IGameManager

        void Client.IGameManager.LatencyUpdated(double _latency)
        {
        }

        void Client.IGameManager.Stopped()
        {
            m_isRunning = false;
            m_updatable.IsRunning = false;
        }

        #endregion Client.IGameManager

        #region ShootStage.IValidationTarget

        IEnumerable<OffenseStage.HitTarget> OffenseStage.IValidationTarget.GetHitTargets(double _time, byte _shooterId)
        {
            return from p
                   in m_players.Values
                   where p.Id != _shooterId && p.ActionHistory.IsAlive(_time)
                   select new OffenseStage.HitTarget { playerId = p.Id, snapshot = p.GetSnapshot(_time) };
        }

        void OffenseStage.IValidationTarget.Offense(double _time, byte _offenderId, byte _offendedId, float _damage, OffenseType _type, Vector3 _origin)
        {
        }

        #endregion ShootStage.IValidationTarget
    }
}