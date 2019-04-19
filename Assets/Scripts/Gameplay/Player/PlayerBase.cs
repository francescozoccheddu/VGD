using UnityEngine;

using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking
{
    internal interface IReadOnlyPlayer
    {
        #region Public Properties

        byte Id { get; }
        PlayerInfo? Info { get; }
        bool IsLocal { get; }
        double LocalTime { get; }
        double TimeOffset { get; }

        // Status
        int Ping { get; }
        int Deaths { get; }
        int Kills { get; }

        IReadOnlyInputHistory InputHistory { get; }
        IReadOnlyLifeHistory LifeHistory { get; }
        IReadOnlyWeaponsHistory WeaponsHistory { get; }
        IReadOnlyMovementHistory MovementHistory { get; }

        #endregion Public Properties
    }

    internal static class PlayerHelper
    {
        #region Public Methods

        public static Snapshot GetSnapshot(this IReadOnlyPlayer _player, double _time)
        {
            return _player.MovementHistory.GetSnapshot(_time, _player.InputHistory);
        }

        #endregion Public Methods
    }

    internal abstract class PlayerBase : IReadOnlyPlayer, EventHistory<SpawnInfo>.ITarget
    {
        #region Public Properties

        public double HistoryDuration { get => m_historyDuration; set { Debug.Assert(value >= 0.0); m_historyDuration = value; } }
        public byte Id { get; }
        public PlayerInfo? Info { get; set; }
        public abstract bool IsLocal { get; }
        public double LocalTime => m_manager.Time + TimeOffset;
        public double TimeOffset { get; set; }

        // Status
        public SingleValueHistory<int> PingValue { get; }
        public SingleValueHistory<int> DeathsValue { get; }
        public SingleValueHistory<int> KillsValue { get; }

        public IReadOnlyLifeHistory LifeHistory => m_lifeHistory;
        public IReadOnlyWeaponsHistory WeaponsHistory => m_weaponsHistory;
        public IReadOnlyMovementHistory MovementHistory => m_movementHistory;
        public IReadOnlyInputHistory InputHistory => m_inputHistory;

        public int Ping => PingValue.Value;
        public int Deaths => DeathsValue.Value;
        public int Kills => KillsValue.Value;

        #endregion Public Properties

        #region Private Fields

        // Logic
        private readonly IPlayerManager m_manager;
        // Components
        private readonly OffenseStage m_offenseStage;
        private readonly PlayerView m_view;
        private readonly LifeHistory m_lifeHistory;
        private readonly EventHistory<SpawnInfo> m_spawnHistory;
        private readonly WeaponsHistory m_weaponsHistory;
        private MovementHistory m_movementHistory;
        private InputHistory m_inputHistory;
        private bool m_isAlive;
        private double m_historyDuration;
        private double m_quitTime;
        private bool m_exploded;

        #endregion Private Fields

        #region Protected Constructors

        protected PlayerBase(IPlayerManager _manager, byte _id)
        {
            // Logic
            Id = _id;
            m_manager = _manager;
            m_view = new PlayerView();
            m_historyDuration = 1.0;
            TimeOffset = 0.0;
            Info = null;
            // Status
            m_movementHistory = new MovementHistory
            {
                MaxPrevisionTime = 0.5
            };
            m_inputHistory = new InputHistory();
            m_lifeHistory = new LifeHistory();
            m_spawnHistory = new EventHistory<SpawnInfo>()
            {
                Target = this
            };
            m_weaponsHistory = new WeaponsHistory();
            KillsValue = new SingleValueHistory<int>();
            DeathsValue = new SingleValueHistory<int>();
            m_quitTime = double.PositiveInfinity;
            PingValue = new SingleValueHistory<int>();
            m_offenseStage = new OffenseStage();
        }

        #endregion Protected Constructors

        #region Public Methods

        public void PutShot(double _time, ShotInfo _info)
        {
            if (_info.isRocket)
            {
                RocketShotOffense offense = new RocketShotOffense(Id, _info.position, _info.sight.Direction);
                m_manager.OffenseBackstage.PutRocket(_time, offense);
                m_offenseStage.Put(_time, offense);
            }
            else
            {
                m_weaponsHistory.CanShootRifle(_time, out float power);
                power = Mathf.Max(Gameplay.Action.WeaponsHistory.c_rifleMinPower, power);
                RifleShotOffense offense = new RifleShotOffense(Id, _info.position, _info.sight.Direction, power);
                m_manager.OffenseBackstage.PutRifle(_time, offense);
                m_offenseStage.Put(_time, offense);
            }
            OnShotScheduled(_time, _info);
        }

        public void PutDamage(double _time, DamageInfo _info)
        {
            m_lifeHistory.PutDamage(_time, _info);
            OnDamageScheduled(_time, _info);
        }

        public void PutKaze(double _time, KazeInfo _info)
        {
            PutDamage(_time, new DamageInfo
            {
                damage = 0,
                maxHealth = Gameplay.Action.LifeHistory.c_explosionHealth,
                offenderId = Id,
                offenseType = OffenseType.Explosion
            });
            OnKazeScheduled(_time, _info);
        }

        public void PutHealth(double _time, int _health)
        {
            m_lifeHistory.PutHealth(_time, _health);
        }

        public void PutQuit(double _time)
        {
            if (!IsQuit(_time))
            {
                m_quitTime = _time;
                OnQuitScheduled(_time);
            }
        }

        public bool IsQuit(double _time)
        {
            return _time >= m_quitTime;
        }

        public void PutSight(int _step, Sight _sight)
        {
            m_movementHistory.Put(_step, _sight);
        }

        public void PutSimulation(int _step, CharacterController _characterController)
        {
            m_movementHistory.Put(_step, _characterController);
        }

        public void PutInput(int _step, InputStep _input)
        {
            m_inputHistory.Put(_step, _input);
        }

        public void PutSpawn(double _time, SpawnInfo _info)
        {
            m_spawnHistory.Put(_time, _info);
            m_weaponsHistory.PutSpawn(_time);
            m_lifeHistory.PutHealth(_time, Gameplay.Action.LifeHistory.c_fullHealth);
            OnSpawnScheduled(_time, _info);
        }

        public void Update()
        {
            m_spawnHistory.PerformUntil(LocalTime);
            int health = m_lifeHistory.GetHealth(LocalTime);
            if (health < 0 && m_isAlive)
            {
                OnActorDied();
            }
            else if (health > 0 && !m_isAlive)
            {
                OnActorSpawned();
            }
            if (!m_exploded && health <= Gameplay.Action.LifeHistory.c_explosionHealth)
            {
                m_exploded = true;
                // TODO Explode view
            }
            m_isAlive = health > 0;
            OnActorBreathed();
            m_offenseStage.Update(LocalTime);
            OnUpdated();
            UpdateView();
            Trim();
        }

        void EventHistory<SpawnInfo>.ITarget.Perform(double _time, SpawnInfo _value)
        {
            Snapshot snapshot = SpawnManager.Get(_value.spawnPoint);
            PutSimulation(_time.SimulationSteps(), snapshot.simulation);
            PutSight(_time.SimulationSteps(), snapshot.sight);
            m_view.Move(snapshot);
            m_view.ReachTarget();
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnQuitScheduled(double _time)
        {
        }

        protected virtual void OnShotScheduled(double _time, ShotInfo _info)
        {
        }

        protected virtual void OnKazeScheduled(double _time, KazeInfo _info)
        {
        }

        protected virtual void OnSpawnScheduled(double _time, SpawnInfo _info)
        {
        }

        protected virtual void OnDamageScheduled(double _time, DamageInfo _info)
        {
        }

        protected virtual void OnActorSpawned()
        {
        }

        protected virtual void OnActorBreathed()
        {
        }

        protected virtual void OnActorDied()
        {
        }

        protected virtual void OnUpdated()
        {
        }

        #endregion Protected Methods

        #region Private Methods

        private void Trim()
        {
            double lastTime = m_manager.Time - HistoryDuration;
            int lastStep = lastTime.SimulationSteps();
            m_inputHistory.Trim(lastStep);
            m_lifeHistory.Trim(lastTime);
            m_weaponsHistory.Trim(lastTime);
            m_movementHistory.ForgetOlder(lastStep, true);
        }

        private void UpdateView()
        {
            if (IsQuit(LocalTime))
            {
                m_view.Destroy();
            }
            else
            {
                m_view.Move(this.GetSnapshot(LocalTime));
                m_view.isAlive = m_lifeHistory.GetHealth(LocalTime) > 0;
                m_view.Update(Time.deltaTime);
            }
        }

        #endregion Private Methods
    }
}