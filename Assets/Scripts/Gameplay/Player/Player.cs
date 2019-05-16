using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.PlayerView;
using Wheeled.Scene;
using Wheeled.Gameplay.Offense;
using Wheeled.Core;

namespace Wheeled.Gameplay.Player
{
    public interface IReadOnlyPlayer
    {
        int Id { get; }
        PlayerInfo? Info { get; }
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
    }

    public static class PlayerHelper
    {
        public static Snapshot GetSnapshot(this IReadOnlyPlayer _player, double _time)
        {
            return _player.MovementHistory.GetSnapshot(_time, _player.InputHistory);
        }

        public static bool IsLocal(this IReadOnlyPlayer _player)
        {
            return _player == GameManager.Current.LocalPlayer;
        }

        public static Color GetColor(this IReadOnlyPlayer _player)
        {
            return Scripts.PlayerPreferences.colors[_player.Info?.color ?? 0];
        }

    }

    public abstract class Player : IReadOnlyPlayer, EventHistory<SpawnInfo>.ITarget, EventHistory<bool>.ITarget
    {
        public double HistoryDuration { get => m_historyDuration; set { Debug.Assert(value >= 0.0); m_historyDuration = value; } }
        public int Id { get; }
        public PlayerInfo? Info
        {
            get => m_info;
            set
            {
                m_info = value;
                if (value != null)
                {
                    m_view.Head = Scripts.PlayerPreferences.heads[value.Value.head].prefab;
                    m_view.Color = Scripts.PlayerPreferences.colors[value.Value.color];
                    OnInfoSetup();
                }
            }
        }
        public double LocalTime => GameManager.Current.Time + TimeOffset;
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

        // Components
        private readonly OffenseStage m_offenseStage;
        private readonly PlayerView.PlayerView m_view;
        private readonly LifeHistory m_lifeHistory;
        private readonly EventHistory<SpawnInfo> m_spawnHistory;
        private readonly WeaponsHistory m_weaponsHistory;
        private readonly EventHistory<bool> m_shootHistory;
        private readonly MovementHistory m_movementHistory;
        private readonly InputHistory m_inputHistory;
        private readonly OffenseBackstage m_offenseBackstage;
        private bool m_isAlive;
        private double m_historyDuration;
        private double m_quitTime;
        private PlayerInfo? m_info;

        protected Player(int _id, OffenseBackstage _offenseBackstage, bool _isLocal)
        {
            // Logic
            Id = _id;
            m_offenseBackstage = _offenseBackstage;
            m_view = new PlayerView.PlayerView()
            {
                IsLocal = _isLocal,
                sightInterpolationQuickness = 5.0f,
                isSightInterpolationEnabled = !_isLocal,
                isPositionInterpolationEnabled = true,
                positionInterpolationQuickness = _isLocal ? 25.0f : 20.0f
            };
            m_shootHistory = new EventHistory<bool>()
            {
                Target = this
            };
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

        public void PutShot(double _time, ShotInfo _info)
        {
            if (_info.isRocket)
            {
                m_weaponsHistory.PutRocketShot(_time);
                RocketShotOffense offense = new RocketShotOffense(Id, _info.position, _info.sight);
                m_offenseBackstage.PutRocket(_time, offense);
                m_offenseStage.Put(_time, offense);
            }
            else
            {
                m_weaponsHistory.PutRifleShot(_time);
                m_weaponsHistory.CanShootRifle(_time, out float power);
                power = Mathf.Max(Action.WeaponsHistory.c_rifleMinPower, power);
                RifleShotOffense offense = new RifleShotOffense(Id, _info.position, _info.sight, power);
                m_offenseBackstage.PutRifle(_time, offense);
                m_offenseStage.Put(_time, offense);
            }
            m_shootHistory.Put(_time, _info.isRocket);
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
                offenseType = EOffenseType.Explosion
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
            int? health = LifeHistory.GetHealthOrNull(LocalTime);
            if (health != null)
            {
                bool isAlive = LifeHistoryHelper.IsAlive(health.Value);
                if (m_isAlive && !isAlive)
                {
                    OnActorDied();
                }
                else if (!m_isAlive && isAlive)
                {
                    OnActorSpawned();
                }
                m_isAlive = isAlive;
                if (isAlive)
                {
                    OnActorBreathed();
                }
            }
            m_offenseStage.Update(LocalTime);
            OnUpdated();
            m_shootHistory.PerformUntil(LocalTime);
            UpdateView(health);
            Trim();
        }

        void EventHistory<SpawnInfo>.ITarget.Perform(double _time, SpawnInfo _value)
        {
            Snapshot snapshot = SpawnManagerBehaviour.Get(_value.spawnPoint);
            PutSimulation(_time.SimulationSteps(), snapshot.simulation);
            PutSight(_time.SimulationSteps(), snapshot.sight);
            m_view.Move(snapshot);
            m_view.ReachTarget();
        }

        void EventHistory<bool>.ITarget.Perform(double _time, bool _value)
        {
            if (!IsQuit(_time))
            {
                if (_value)
                {
                    m_view.ShootRocket();
                }
                else
                {
                    m_view.ShootRifle();
                }
            }
        }

        public void Destroy()
        {
            m_view.Destroy();
        }

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

        protected virtual void OnInfoSetup()
        {
        }

        private void Trim()
        {
            double lastTime = GameManager.Current.Time - HistoryDuration;
            int lastStep = lastTime.SimulationSteps();
            m_inputHistory.Trim(lastStep);
            m_lifeHistory.Trim(lastTime);
            m_weaponsHistory.Trim(lastTime);
            m_movementHistory.Trim(lastStep);
        }

        private void UpdateView(int? _health)
        {
            if (IsQuit(LocalTime))
            {
                m_view.Destroy();
            }
            else
            {
                m_weaponsHistory.CanShootRifle(LocalTime, out float riflePower);
                m_view.RiflePower = riflePower;
                m_view.Move(this.GetSnapshot(LocalTime));
                m_view.State = LifeHistoryHelper.GetLifeState(_health);
                m_view.Update(Time.deltaTime);
            }
        }
    }

    public struct PlayerInfo
    {
        public string name;
        public int head;
        public int color;
    }
}