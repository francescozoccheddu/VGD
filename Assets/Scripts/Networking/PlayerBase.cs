using System.Collections.Generic;
using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking
{
    internal abstract class PlayerBase : ActionHistory.ITarget
    {
        private readonly ActionHistory m_actionHistory;
        private readonly InputHistory m_inputHistory;
        private readonly IGameManager m_manager;
        private readonly MovementHistory m_movementHistory;
        private readonly PlayerView m_view;
        private double m_historyDuration;
        private double m_lastPingTime;
        private double m_spawnDelay;

        protected PlayerBase(IGameManager _manager, byte _id)
        {
            Id = _id;
            m_manager = _manager;
            m_movementHistory = new MovementHistory();
            m_inputHistory = new InputHistory();
            m_actionHistory = new ActionHistory
            {
                Target = this
            };
            m_view = new PlayerView();
            m_historyDuration = 1.0;
            m_lastPingTime = double.NegativeInfinity;
            m_spawnDelay = 0.0;
            m_ShouldHandleRespawn = false;
            Info = null;
            Ping = 0;
        }

        public double HistoryDuration { get => m_historyDuration; set { Debug.Assert(value >= 0.0); m_historyDuration = value; } }
        public byte Id { get; }
        public PlayerInfo? Info { get; private set; }
        public abstract bool IsLocal { get; }
        public int Ping { get; private set; }
        public Snapshot Snapshot { get; private set; }
        public double SpawnDelay { get => m_spawnDelay; set { Debug.Assert(value >= 0.0); m_spawnDelay = value; } }
        public double TimeOffset { get; set; }
        public ActionHistory.IState State => m_actionHistory.State;
        protected double m_LocalTime => m_manager.Time + TimeOffset;
        protected bool m_ShouldHandleRespawn { get; set; }

        #region GameManager interface

        public void Introduce(PlayerInfo _info)
        {
            Info = _info;
        }

        public void PutDeaths(double _time, int _deaths)
        {
            m_actionHistory.PutDeaths(_time, _deaths);
        }

        public void PutHealth(double _time, int _health)
        {
            m_actionHistory.PutHealth(_time, _health);
        }

        public void PutKills(double _time, int _kills)
        {
            m_actionHistory.PutKills(_time, _kills);
        }

        public void PutPing(double _time, int _ping)
        {
            if (m_lastPingTime > _time == false)
            {
                Ping = _ping;
                m_lastPingTime = _time;
            }
        }

        public void PutQuit(double _time)
        {
            m_actionHistory.PutQuit(_time);
        }

        public void Update()
        {
            m_actionHistory.Update(m_LocalTime);
            if (m_ShouldHandleRespawn)
            {
                HandleRespawn();
            }
            OnUpdated();
            UpdateSnapshot();
            m_actionHistory.Perform();
            UpdateView();
            Trim();
        }

        #endregion GameManager interface

        #region ActionHistory.ITarget

        void ActionHistory.ITarget.PerformDamage(double _time, DamageInfo _info)
        {
        }

        void ActionHistory.ITarget.PerformDeath(double _time, DeathInfo _info)
        {
        }

        void ActionHistory.ITarget.PerformHitConfirm(double _time, HitConfirmInfo _info)
        {
        }

        void ActionHistory.ITarget.PerformRifleShoot(double _time, ShotInfo _info, float _power)
        {
            m_manager.ShootStage.ShootRifle(_time, _info.position, _info.sight.Direction, Id, _power);
        }

        void ActionHistory.ITarget.PerformRocketShoot(double _time, ShotInfo _info)
        {
        }

        void ActionHistory.ITarget.PerformSpawn(double _time, SpawnInfo _info)
        {
        }

        #endregion ActionHistory.ITarget

        #region Protected abstract events

        protected virtual void OnDamageScheduled(double _time, DamageInfo _info)
        {
        }

        protected virtual void OnDeathScheduled(double _time, DeathInfo _info)
        {
        }

        protected virtual void OnHitConfirmScheduled(double _time, HitConfirmInfo _info)
        {
        }

        protected virtual void OnQuitScheduled(double _time)
        {
        }

        protected virtual void OnShootScheduled(double _time, ShotInfo _info)
        {
        }

        protected virtual void OnSpawnScheduled(double _time, SpawnInfo _info)
        {
        }

        protected virtual void OnUpdated()
        {
        }

        #endregion Protected abstract events

        #region Protected services

        protected SimulationStep CorrectSimulation(int _step, InputStep _input, SimulationStep _simulation)
        {
            PutInput(_step, _input);
            return m_inputHistory.SimulateFrom(_step, _simulation);
        }

        protected IEnumerable<InputStep> GetReversedInputSequence(int _step, int _maxStepsCount)
        {
            return m_inputHistory.GetReversedInputSequence(_step, _maxStepsCount);
        }

        protected void PutDamage(double _time, DamageInfo _info)
        {
            if (IsLocal)
            {
                m_actionHistory.PutDamage(_time, _info);
            }
            OnDamageScheduled(_time, _info);
        }

        protected void PutDeath(double _time, DeathInfo _info)
        {
            m_actionHistory.PutDeath(_time, _info);
            OnDeathScheduled(_time, _info);
        }

        protected void PutHitConfirm(double _time, HitConfirmInfo _info)
        {
            if (IsLocal)
            {
                m_actionHistory.PutHitConfirm(_time, _info);
            }
            OnHitConfirmScheduled(_time, _info);
        }

        protected void PutInput(int _step, in InputStep _inputStep)
        {
            m_inputHistory.Put(_step, _inputStep);
        }

        protected void PutShoot(double _time, ShotInfo _info)
        {
            m_actionHistory.PutShot(_time, _info);
            OnShootScheduled(_time, _info);
        }

        protected void PutSight(int _step, in Sight _sight)
        {
            m_movementHistory.Put(_step, _sight);
        }

        protected void PutSimulation(int _step, in SimulationStep _simulation)
        {
            m_movementHistory.Put(_step, _simulation);
        }

        protected void PutSpawn(double _time, SpawnInfo _info)
        {
            m_actionHistory.PutSpawn(_time, _info);
            OnSpawnScheduled(_time, _info);
        }

        #endregion Protected services

        #region Component updates

        private void HandleRespawn()
        {
            if (m_actionHistory.ShouldSpawn)
            {
                double spawnTime = m_LocalTime + m_spawnDelay;
                SpawnInfo info = new SpawnInfo
                {
                    spawnPoint = 0 // TODO Decide where to spawn
                };
                PutSpawn(spawnTime, info);
                OnSpawnScheduled(spawnTime, info);
            }
        }

        private void Trim()
        {
            double lastTime = m_manager.Time - HistoryDuration;
            int lastStep = lastTime.SimulationSteps();
            m_inputHistory.Trim(lastStep);
            m_actionHistory.Trim(lastTime);
            m_movementHistory.ForgetOlder(lastStep, true);
        }

        private void UpdateSnapshot()
        {
            m_movementHistory.GetSimulation(m_LocalTime, out SimulationStep? simulation, m_inputHistory);
            if (simulation != null)
            {
                Snapshot = new Snapshot
                {
                    simulation = simulation.Value,
                    sight = Snapshot.sight
                };
            }
            m_movementHistory.GetSight(m_LocalTime, out Sight? sight);
            if (sight != null)
            {
                Snapshot = new Snapshot
                {
                    simulation = Snapshot.simulation,
                    sight = sight.Value
                };
            }
        }

        private void UpdateView()
        {
            if (m_actionHistory.IsQuit)
            {
                m_view.Destroy();
            }
            else
            {
                m_view.Move(Snapshot);
                m_view.isAlive = m_actionHistory.IsAlive;
                m_view.Update(Time.deltaTime);
            }
        }

        #endregion Component updates
    }
}