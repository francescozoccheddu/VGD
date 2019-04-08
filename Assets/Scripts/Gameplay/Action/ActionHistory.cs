using System.Linq;
using UnityEngine;

using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{

    internal interface IActionHistory
    {
        bool CanShootRocket(double _time);

        bool CanKaze(double _time);

        int GetDeaths(double _time);

        int GetHealth(double _time);

        int GetKills(double _time);

        float GetRiflePower(double _time);

        bool IsFirstSpawned(double _time);

        bool IsQuit(double _time);

        bool ShouldSpawn(double _time, out int _outHealth);
    }

    internal static class ActionHistoryHelpers
    {

        public static bool IsAlive(this IActionHistory _actionHistory, double _time)
        {
            return !_actionHistory.IsQuit(_time) && _actionHistory.GetHealth(_time) > 0;
        }
        public static bool IsFirstSpawnedAndNotQuit(this IActionHistory _actionHistory, double _time)
        {
            return _actionHistory.IsFirstSpawned(_time) && !_actionHistory.IsQuit(_time);
        }

        public static bool CanShootRifle(this IActionHistory _actionHistory, double _time)
        {
            return _actionHistory.GetRiflePower(_time) > 0.0f;
        }

        public static bool CanShootRifle(this IActionHistory _actionHistory, double _time, out float _outPower)
        {
            _outPower = _actionHistory.GetRiflePower(_time);
            return _outPower > 0.0f;
        }

    }

    internal sealed class ActionHistory : IActionHistory
    {

        public const int c_fullHealth = 100;
        public const int c_explosionhealth = -50;
        public const float c_respawnWaitTime = 2.0f;
        public const float c_rifleCooldown = 0.5f;
        public const float c_riflePowerUpTime = 3.0f;
        public const float c_rocketCooldown = 1.0f;
        private readonly LinkedListHistory<double, IAction> m_actionHistory = new LinkedListHistory<double, IAction>();
        private readonly LinkedListHistory<double, int> m_deathCountHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListSimpleHistory<double> m_explosionHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListHistory<double, int> m_healthHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListHistory<double, int> m_killCountHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListSimpleHistory<double> m_rifleShootHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListSimpleHistory<double> m_rocketShootHistory = new LinkedListSimpleHistory<double>();
        private double? m_firstAliveTime;
        private double? m_quitTime;

        public ActionHistory(byte _id)
        {
            Id = _id;
        }

        public byte Id { get; }

        public StaticQuery GetQuery(double _time)
        {
            return new StaticQuery(this, _time);
        }

        #region Put

        public void PutDamage(double _time, DamageInfo _info)
        {
            m_actionHistory.Add(_time, new DamageAction { info = _info });
        }

        public void PutDeath(double _time, DeathInfo _info)
        {
            m_actionHistory.Add(_time, new DeathAction { info = _info });
            PutHealth(_time, 0);
            if (_info.isExploded)
            {
                m_explosionHistory.Set(_time);
            }
        }

        public void PutDeaths(double _time, int _deaths)
        {
            m_deathCountHistory.Set(_time, _deaths);
        }

        public void PutHealth(double _time, int _health)
        {
            m_healthHistory.Set(_time, _health);
            if (_health > 0 && m_firstAliveTime >= _time != true)
            {
                m_firstAliveTime = _time;
            }
        }

        public void PutHitConfirm(double _time, HitConfirmInfo _info)
        {
            m_actionHistory.Set(_time, new HitConfirmAction { info = _info });
        }

        public void PutKills(double _time, int _kills)
        {
            m_killCountHistory.Set(_time, _kills);
        }

        public void PutQuit(double _time)
        {
            if (!(m_quitTime < _time))
            {
                m_quitTime = _time;
            }
        }

        public void PutShot(double _time, ShotInfo _info)
        {
            m_actionHistory.Add(_time, new ShootAction() { info = _info });
            if (_info.isRocket)
            {
                m_rocketShootHistory.Set(_time);
            }
            else
            {
                m_rifleShootHistory.Set(_time);
            }
        }

        public void PutSpawn(double _time, SpawnInfo _info)
        {
            m_actionHistory.Add(_time, new SpawnAction { info = _info });
            PutHealth(_time, c_fullHealth);
        }

        #endregion Put

        public void Trim(double _time)
        {
            m_actionHistory.ForgetOlder(_time, true);
            m_healthHistory.ForgetOlder(_time, true);
            m_explosionHistory.ForgetOlder(_time, true);
            m_rifleShootHistory.ForgetOlder(_time, true);
            m_rocketShootHistory.ForgetOlder(_time, true);
            m_killCountHistory.ForgetOlder(_time, true);
            m_deathCountHistory.ForgetOlder(_time, true);
        }

        #region Actions

        public interface ITarget
        {
            void PerformDamage(double _time, DamageInfo _info);

            void PerformDeath(double _time, DeathInfo _info);

            void PerformHitConfirm(double _time, HitConfirmInfo _info);

            void PerformRifleShoot(double _time, ShotInfo _info, float _power);

            void PerformRocketShoot(double _time, ShotInfo _info);

            void PerformSpawn(double _time, SpawnInfo _info);
        }

        private interface IAction
        {
            void Perform(double _time, ActionHistory _history);
        }

        public ITarget Target { get; set; }

        public void PerformUntil(double _time)
        {
            foreach ((double time, IAction action) in m_actionHistory.GetFullSequence().Where(_n => _n.time <= _time))
            {
                action.Perform(time, this);
            }
            m_actionHistory.ForgetAndOlder(_time);
        }

        private struct DamageAction : IAction
        {
            public DamageInfo info;

            void IAction.Perform(double _time, ActionHistory _history)
            {
                _history.Target?.PerformDamage(_time, info);
            }
        }

        private struct DeathAction : IAction
        {
            public DeathInfo info;

            void IAction.Perform(double _time, ActionHistory _history)
            {
                bool kaze = info.isExploded && info.offenseType == OffenseType.Explosion && info.killerId == _history.Id;
                if ((!kaze && _history.CanDie(_time, true)) || (kaze && _history.CanKaze(_time, true)))
                {
                    _history.Target?.PerformDeath(_time, info);
                }
            }
        }

        private struct HitConfirmAction : IAction
        {
            public HitConfirmInfo info;

            void IAction.Perform(double _time, ActionHistory _history)
            {
                _history.Target?.PerformHitConfirm(_time, info);
            }
        }

        private struct ShootAction : IAction
        {
            public ShotInfo info;

            void IAction.Perform(double _time, ActionHistory _history)
            {
                if (info.isRocket && _history.CanShootRocket(_time, true))
                {
                    _history.Target?.PerformRocketShoot(_time, info);
                }
                else
                {
                    float power = _history.GetRiflePower(_time, true);
                    if (power > 0.0f)
                    {
                        _history.Target?.PerformRifleShoot(_time, info, power);
                    }
                }
            }
        }

        private struct SpawnAction : IAction
        {
            public SpawnInfo info;

            void IAction.Perform(double _time, ActionHistory _history)
            {
                _history.Target?.PerformSpawn(_time, info);
            }
        }

        #endregion Actions

        #region Query

        public bool CanShootRocket(double _time)
        {
            return CanShootRocket(_time, false);
        }

        public bool CanKaze(double _time)
        {
            return CanKaze(_time, false);
        }

        public int GetDeaths(double _time)
        {
            return m_deathCountHistory.GetOrPrevious(_time)?.value ?? 0;
        }

        public int GetHealth(double _time)
        {
            return m_healthHistory.GetOrPrevious(_time)?.value ?? 0;
        }

        public int GetKills(double _time)
        {
            return m_killCountHistory.GetOrPrevious(_time)?.value ?? 0;
        }

        public float GetRiflePower(double _time)
        {
            return GetRiflePower(_time, false);
        }

        public bool IsFirstSpawned(double _time)
        {
            return _time >= m_firstAliveTime;
        }

        public bool IsQuit(double _time)
        {
            return m_quitTime <= _time;
        }

        public bool ShouldSpawn(double _time, out int _outHealth)
        {
            _outHealth = 0;
            bool shouldSpawn = true;
            bool healthSet = false;
            double lastTime = double.NegativeInfinity;
            foreach (HistoryNode<double, int> node in m_healthHistory.GetFullReversedSequence())
            {
                bool alive = node.value > 0;
                if (node.time <= _time)
                {
                    if (!healthSet)
                    {
                        healthSet = true;
                        _outHealth = node.value;
                        shouldSpawn &= !alive;
                    }
                    lastTime = node.time;
                    if (!shouldSpawn || alive || _time - lastTime >= c_respawnWaitTime)
                    {
                        break;
                    }
                }
                else
                {
                    shouldSpawn &= !alive;
                }
            }
            shouldSpawn &= _time - lastTime >= c_respawnWaitTime;
            return shouldSpawn;
        }

        private bool CanDie(double _time, bool _isValidation)
        {
            return !IsQuit(_time) &&
                m_healthHistory
                .GetReversedSequenceSince(_time, false, true)
                .Where(_n => !_isValidation || _n.time != _time)
                .Cast<HistoryNode<double, int>?>()
                .FirstOrDefault()
                ?.value > 0;
        }

        private bool CanKaze(double _time, bool _isValidation)
        {
            if (this.IsFirstSpawnedAndNotQuit(_time))
            {
                double? lastExplosion = m_explosionHistory
                    .GetReversedSequenceSince(_time, false, true)
                    .Cast<double?>()
                    .FirstOrDefault(_t => !_isValidation || _t != _time);
                return lastExplosion == null ||
                    m_healthHistory
                    .GetReversedSequenceSince(_time, false, true)
                    .Where(_n => !_isValidation || _n.time != _time)
                    .TakeWhile(_n => _n.time > lastExplosion)
                    .Any(_n => _n.value > 0);
            }
            return false;
        }

        private bool CanShootRocket(double _time, bool _isValidation)
        {
            if (this.IsAlive(_time))
            {
                double? lastShot = m_rocketShootHistory
                    .GetReversedSequenceSince(_time, false, true)
                    .Cast<double?>()
                    .FirstOrDefault(_t => !_isValidation || _t != _time);
                return !(_time - lastShot < c_rocketCooldown && GetCurrentSpawnTime(_time, lastShot.Value) == null);
            }
            return false;
        }

        private double? GetCurrentSpawnTime(double _time, double _minTime = double.NegativeInfinity)
        {
            double? respawnTime = null;
            foreach (HistoryNode<double, int> node in m_healthHistory.GetReversedSequenceSince(_time, false, true))
            {
                if (node.value > 0)
                {
                    respawnTime = node.time;
                }
                else
                {
                    break;
                }
                if (respawnTime < _minTime)
                {
                    return null;
                }
            }
            return respawnTime;
        }

        private float GetRiflePower(double _time, bool _isValidation)
        {
            if (this.IsAlive(_time))
            {
                double? lastShot = m_rifleShootHistory
                    .GetReversedSequenceSince(_time, false, true)
                    .Cast<double?>()
                    .FirstOrDefault(_t => !_isValidation || _t != _time);
                if (_time - lastShot < c_rifleCooldown + c_riflePowerUpTime && GetCurrentSpawnTime(_time, lastShot.Value) == null)
                {
                    return Mathf.Clamp01((float) ((_time - lastShot.Value - c_rifleCooldown) / c_riflePowerUpTime));
                }
                else
                {
                    return 1.0f;
                }
            }
            return 0.0f;
        }

        public class StaticQuery
        {
            public StaticQuery(ActionHistory _history, double _time)
            {
                IsFirstSpawned = _history.IsFirstSpawned(_time);
                IsQuit = _history.IsQuit(_time);
                ShouldSpawn = _history.ShouldSpawn(_time, out int health);
                Health = health;
                CanKaze = _history.CanKaze(_time);
                Kills = _history.GetKills(_time);
                Deaths = _history.GetDeaths(_time);
                RiflePower = _history.GetRiflePower(_time);
                CanShootRocket = _history.CanShootRocket(_time);
                Time = _time;
            }

            public double Time { get; }
            public bool IsFirstSpawned { get; }
            public bool IsFirstSpawnedAndNotQuit => IsFirstSpawned && !IsQuit;
            public bool CanKaze { get; }
            public float RiflePower { get; }
            public bool CanShootRifle => RiflePower > 0.0f;
            public bool CanShootRocket { get; }
            public int Deaths { get; }
            public int Health { get; }
            public bool IsAlive => Health > 0 && !IsQuit;
            public bool IsQuit { get; }
            public int Kills { get; }
            public bool ShouldSpawn { get; }
        }

        #endregion Query
    }
}