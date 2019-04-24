using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Stage;
using Wheeled.HUD;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager, IPlayerManager, OffenseBackstage.IValidationTarget
    {
        #region Private Classes

        private abstract class ClientPlayer : Player
        {
            #region Private Fields

            private readonly ClientGameManager m_manager;

            private bool m_isQuit;

            #endregion Private Fields

            #region Protected Constructors

            protected ClientPlayer(ClientGameManager _manager, byte _id, OffenseBackstage _offenseBackstage) : base(_manager, _id, _offenseBackstage)
            {
                m_manager = _manager;
            }

            #endregion Protected Constructors

            #region Public Methods

            public void NotifyQuit()
            {
                if (!m_isQuit)
                {
                    m_isQuit = true;
                    m_manager.MatchBoard.Put(m_manager.m_time, new MatchBoard.QuitEvent
                    {
                        player = this
                    });
                }
            }

            #endregion Public Methods

            #region Protected Methods

            protected override void OnUpdated()
            {
                base.OnUpdated();
                if (!m_isQuit && IsQuit(m_manager.m_time))
                {
                    NotifyQuit();
                }
            }

            #endregion Protected Methods
        }

        #endregion Private Classes

        #region Public Properties

        double IPlayerManager.Time => m_time;
        public MatchBoard MatchBoard { get; }

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
        private const double c_quitForgetDelay = 5.0;
        private readonly OffenseBackstage m_offenseBackstage;
        private readonly OffenseBackstage m_localOffenseBackstage;
        private readonly LocalPlayer m_localPlayer;
        private readonly Dictionary<byte, ClientPlayer> m_players;
        private readonly Client.IServer m_server;
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
                MaxMovementNotifyFrequency = 15
            };
            m_players = new Dictionary<byte, ClientPlayer>
            {
                { _id, m_localPlayer }
            };
            MatchBoard = new MatchBoard();
            // Ready notify
            Serializer.WriteReady();
            m_server.Send(NetworkManager.SendMethod.ReliableUnordered);
            UpdateScoreBoard();
        }

        #endregion Public Constructors

        #region Public Methods

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
                foreach (KeyValuePair<byte, ClientPlayer> p in (from p in m_players where p.Value.IsQuit(forgetTime) select p).ToList())
                {
                    p.Value.NotifyQuit();
                    p.Value.Destroy();
                    m_players.Remove(p.Key);
                }
                MatchBoard.UpdateUntil(m_time);
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
            return from p
                  in m_players.Values
                   where p.Id != _offense.OffenderId && p.LifeHistory.IsAlive(_time) && !p.IsQuit(_time)
                   select new OffenseBackstage.HitTarget { playerId = p.Id, snapshot = p.GetSnapshot(_time + p.TimeOffset) };
        }

        void OffenseBackstage.IValidationTarget.Damage(double _time, byte _offendedId, Offense _offense, float _damage)
        {
        }

        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense)
        {
            if (m_players.TryGetValue(_offense.OffenderId, out ClientPlayer player))
            {
                return player?.IsQuit(_time) == false;
            }
            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateScoreBoard()
        {
            IEnumerable<ClientPlayer> players = from p
                                                in m_players
                                                where !p.Value.IsQuit(m_time)
                                                select p.Value;
            ScoreBoardBehaviour.Update(players);
        }

        private Player GetOrCreatePlayer(byte _id)
        {
            if (m_players.TryGetValue(_id, out ClientPlayer player))
            {
                return player;
            }
            else
            {
                NetPlayer newNetPlayer = new NetPlayer(this, _id, m_offenseBackstage)
                {
                    HistoryDuration = 2.0,
                };
                m_players.Add(_id, newNetPlayer);
                MatchBoard.Put(m_time, new MatchBoard.JoinEvent
                {
                    player = newNetPlayer
                });
                UpdateScoreBoard();
                return newNetPlayer;
            }
        }

        #endregion Private Methods
    }
}