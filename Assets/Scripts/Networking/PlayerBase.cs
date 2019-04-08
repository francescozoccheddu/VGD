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

        public bool IsActionDelayEnabled { get; set; }

        protected PlayerBase(IGameManager _manager, byte _id)
        {
            Id = _id;
            m_manager = _manager;
            m_movementHistory = new MovementHistory
            {
                MaxPrevisionTime = 0.5
            };
            m_inputHistory = new InputHistory();
            m_actionHistory = new ActionHistory(_id)
            {
                Target = this,
            };
            m_view = new PlayerView();
            m_historyDuration = 1.0;
            TimeOffset = 0.0;
            m_lastPingTime = double.NegativeInfinity;
            m_spawnDelay = 0.0;
            m_ShouldHandleRespawn = false;
            Info = null;
            Ping = 0;
            IsActionDelayEnabled = true;
        }

        public ActionHistory.StaticQuery ActionHistoryLocalTimeQuery { get; private set; }
        public IActionHistory ActionHistory => m_actionHistory;
        public double HistoryDuration { get => m_historyDuration; set { Debug.Assert(value >= 0.0); m_historyDuration = value; } }
        public byte Id { get; }
        public PlayerInfo? Info { get; private set; }
        public abstract bool IsLocal { get; }
        public double LocalTime => m_manager.Time + TimeOffset;
        public int Ping { get; private set; }
        public double SpawnDelay { get => m_spawnDelay; set { Debug.Assert(value >= 0.0); m_spawnDelay = value; } }
        public double TimeOffset { get; set; }
        protected bool m_ShouldHandleRespawn { get; set; }

        #region GameManager interface

        public Snapshot GetSnapshot(double _time)
        {
            Snapshot snapshot = new Snapshot();
            m_movementHistory.GetSimulation(_time, out SimulationStep? simulation, m_inputHistory);
            if (simulation != null)
            {
                snapshot.simulation = simulation.Value;
            }
            m_movementHistory.GetSight(_time, out Sight? sight);
            if (sight != null)
            {
                snapshot.sight = sight.Value;
            }
            return snapshot;
        }

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

        public void PutDamage(double _time, DamageInfo _info)
        {
            if (IsLocal)
            {
                m_actionHistory.PutDamage(_time, _info);
            }
            OnDamageScheduled(_time, _info);
        }

        public void PutDeath(double _time, DeathInfo _info)
        {
            m_actionHistory.PutDeath(_time, _info);
            OnDeathScheduled(_time, _info);
        }

        public void PutHitConfirm(double _time, HitConfirmInfo _info)
        {
            if (IsLocal)
            {
                m_actionHistory.PutHitConfirm(_time, _info);
            }
            OnHitConfirmScheduled(_time, _info);
        }

        public void Update()
        {
            ActionHistoryLocalTimeQuery = m_actionHistory.GetQuery(LocalTime);
            if (m_ShouldHandleRespawn)
            {
                HandleRespawn();
            }
            OnUpdated();
            m_actionHistory.PerformUntil(IsActionDelayEnabled ? LocalTime : m_manager.Time);
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
            if (_info.isExploded)
            {
                m_manager.ShootStage.Explode(_time, _info.killerId, _info.position);
            }
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
            m_manager.ShootStage.ShootRocket(_time, _info.position, _info.sight.Direction, Id);
        }

        void ActionHistory.ITarget.PerformSpawn(double _time, SpawnInfo _info)
        {
            Snapshot snapshot = SpawnManager.Get(_info.spawnPoint);
            PutSimulation(_time.SimulationSteps(), snapshot.simulation);
            PutSight(_time.SimulationSteps(), snapshot.sight);
            m_view.Move(snapshot);
            m_view.ReachTarget();
            OnSpawn(_time, snapshot);
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

        protected virtual void OnSpawn(double _time, Snapshot _snapshot)
        {

        }

        protected virtual void OnUpdated()
        {
        }

        protected virtual void OnExplosion(double _time, Vector3 _position)
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
            if (ActionHistoryLocalTimeQuery.ShouldSpawn)
            {
                double spawnTime = LocalTime + m_spawnDelay;
                SpawnInfo info = new SpawnInfo
                {
                    spawnPoint = SpawnManager.Spawn()
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

        private void UpdateView()
        {
            if (ActionHistoryLocalTimeQuery.IsQuit)
            {
                m_view.Destroy();
            }
            else
            {
                m_view.Move(GetSnapshot(LocalTime));
                m_view.isAlive = ActionHistoryLocalTimeQuery.IsAlive;
                m_view.Update(Time.deltaTime);
            }
        }

        #endregion Component updates
    }
}