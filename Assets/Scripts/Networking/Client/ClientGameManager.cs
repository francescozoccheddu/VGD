using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager, IPlayerManager, OffenseStage.IValidationTarget
    {
        private const double c_localOffset = 0.025;
        public const double c_netOffset = 0.025;
        private const float c_timeSmoothQuickness = 0.2f;
        private readonly LocalPlayer m_localPlayer;
        private IEnumerable<NetPlayer> m_NetPlayers => m_players.Values.Where(_p => _p != m_localPlayer).Cast<NetPlayer>();
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
                MaxMovementNotifyFrequency = 15
            };
            m_players = new Dictionary<byte, Player>
            {
                { _id, m_localPlayer }
            };
            // Ready notify
            Serializer.WriteReady();
            m_server.Send(NetworkManager.SendMethod.ReliableUnordered);
        }

        OffenseStage IPlayerManager.ShootStage => m_shootStage;

        double IPlayerManager.Time => m_time;

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
            foreach (Player p in m_players.Values)
            {
                p.Update();
            }
            m_shootStage.Update(m_time);
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