using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionHistory
    {
        private const int c_fullHealth = 100;
        private const float c_respawnWaitTime = 2.0f;
        private const float c_rifleCooldown = 1.0f;
        private const float c_riflePowerUpTime = 3.0f;
        private const float c_rocketCooldown = 1.0f;
        private readonly LinkedListHistory<double, IAction> m_actionHistory = new LinkedListHistory<double, IAction>();
        private readonly LinkedListHistory<double, int> m_deathCountHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListSimpleHistory<double> m_explosionHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListHistory<double, int> m_healthHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListHistory<double, int> m_killCountHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListSimpleHistory<double> m_killsHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListSimpleHistory<double> m_rifleShootHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListSimpleHistory<double> m_rocketShootHistory = new LinkedListSimpleHistory<double>();
        private double? m_quitTime;

        public ActionHistory(byte _id)
        {
            Id = _id;
            Query = new ImmediateQuery(this);
        }

        public byte Id { get; }
        public ImmediateQuery Query { get; }

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
        }

        public void PutHitConfirm(double _time, HitConfirmInfo _info)
        {
            m_actionHistory.Add(_time, new HitConfirmAction { info = _info });
        }

        public void PutKill(double _time)
        {
            m_killsHistory.Add(_time);
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
                m_rocketShootHistory.Add(_time);
            }
            else
            {
                m_rifleShootHistory.Add(_time);
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
            void Perform(double _time, ActionHistory _history, ITarget _target);
        }

        public ITarget Target { get; set; }

        public void PerformUntil(double _time)
        {
            foreach ((double time, IAction action) in m_actionHistory.GetFullSequence().Where(_n => _n.time <= _time))
            {
                action.Perform(time, this, Target);
            }
            m_actionHistory.ForgetAndOlder(_time);
        }

        private struct DamageAction : IAction
        {
            public DamageInfo info;

            void IAction.Perform(double _time, ActionHistory _history, ITarget _target)
            {
                _target?.PerformDamage(_time, info);
            }
        }

        private struct DeathAction : IAction
        {
            public DeathInfo info;

            void IAction.Perform(double _time, ActionHistory _history, ITarget _target)
            {
                bool IsAliveOrDying()
                {
                    foreach (HistoryNode<double, int> node in _history.m_healthHistory.GetReversedSequenceSince(_time, false, true))
                    {
                        if (node.value > 0)
                        {
                            return true;
                        }
                        else if (node.time != _time)
                        {
                            break;
                        }
                    }
                    return false;
                }
                if (info.isExploded && info.offenseType == OffenseType.Kaze && info.killerId == _history.Id)
                {
                    double? lastExplosion = _history.m_explosionHistory
                        .GetReversedSequenceSince(_time, false, true)
                        .Where(_t => _t < _time)
                        .Cast<double?>()
                        .FirstOrDefault();
                    if (lastExplosion != null)
                    {
                        if (_history.m_healthHistory
                            .GetSequenceSince(lastExplosion.Value, false, true)
                            .Where(_node => _node.time <= _time)
                            .Any(_node => _node.value > 0))
                        {
                            _target?.PerformDeath(_time, info);
                        }
                    }
                    else if (IsAliveOrDying())
                    {
                        _target?.PerformDeath(_time, info);
                    }
                }
                else if (IsAliveOrDying())
                {
                    _target?.PerformDeath(_time, info);
                }
            }
        }

        private struct HitConfirmAction : IAction
        {
            public HitConfirmInfo info;

            void IAction.Perform(double _time, ActionHistory _history, ITarget _target)
            {
                _target?.PerformHitConfirm(_time, info);
            }
        }

        private struct ShootAction : IAction
        {
            public ShotInfo info;

            void IAction.Perform(double _time, ActionHistory _history, ITarget _target)
            {
                if (info.isRocket && _history.Query.CanShootRocket(_time))
                {
                    _target?.PerformRocketShoot(_time, info);
                }
                else if (_history.Query.CanShootRifle(_time, out float power))
                {
                    _target?.PerformRifleShoot(_time, info, power);
                }
            }
        }

        private struct SpawnAction : IAction
        {
            public SpawnInfo info;

            void IAction.Perform(double _time, ActionHistory _history, ITarget _target)
            {
                _target?.PerformSpawn(_time, info);
            }
        }

        #endregion Actions

        #region Query

        private bool CouldShootRifle(double _time, out float _power)
        {
            double? lastRifleShot = m_rocketShootHistory.GetOrPrevious(_time);
            if (lastRifleShot != null)
            {
                double elapsed = _time - lastRifleShot.Value;
                _power = Mathf.Clamp01((float) (elapsed / c_riflePowerUpTime));
                return elapsed >= c_rifleCooldown;
            }
            else
            {
                _power = 1.0f;
                return true;
            }
        }

        private bool CouldShootRocket(double _time)
        {
            double? lastRocketShot = m_rocketShootHistory.GetOrPrevious(_time);
            if (lastRocketShot != null)
            {
                double elapsed = _time - lastRocketShot.Value;
                return elapsed >= c_rocketCooldown;
            }
            else
            {
                return true;
            }
        }

        private int GetDeaths(double _time)
        {
            HistoryNode<double, int>? node = m_deathCountHistory.GetOrPrevious(_time);
            int deaths = node?.value ?? 0;
            IEnumerable<HistoryNode<double, int>> sequence = node != null ? m_healthHistory.GetSequenceSince(node.Value.time, true, true) : m_healthHistory.GetFullSequence();
            bool alive = false;
            foreach (HistoryNode<double, int> healthNode in sequence.Where(_n => _n.time <= _time))
            {
                if (alive && healthNode.value <= 0 && node?.time >= healthNode.time != true)
                {
                    deaths++;
                }
                alive = healthNode.value > 0;
            }
            return deaths;
        }

        private int GetHealth(double _time)
        {
            return m_healthHistory.GetOrPrevious(_time)?.value ?? 0;
        }

        private int GetKills(double _time)
        {
            HistoryNode<double, int>? node = m_killCountHistory.GetOrPrevious(_time);
            int kills = node?.value ?? 0;
            foreach (double time in m_killsHistory.GetReversedSequenceSince(_time, false, true).Where(_n => node?.time >= _n != true))
            {
                kills++;
            }
            return kills;
        }

        private void GetLifeInfo(double _time, out int _outHealth, out bool _outShouldSpawn, out bool _outCanKaze)
        {
            _outHealth = 0;
            double lastTime = 0.0;
            bool found = false;
            bool willSpawn = false;
            double lastAliveTime = _time;
            foreach (HistoryNode<double, int> node in m_healthHistory.GetFullReversedSequence())
            {
                if (node.time <= _time)
                {
                    if (!found)
                    {
                        _outHealth = node.value;
                        found = true;
                    }
                    if (node.value == 0)
                    {
                        lastTime = node.time;
                    }
                    else
                    {
                        lastAliveTime = node.time;
                        break;
                    }
                }
                else
                {
                    willSpawn |= node.value > 0;
                }
            }
            _outShouldSpawn = _outHealth == 0 && (_time - lastTime) >= c_respawnWaitTime && !willSpawn;
            _outCanKaze = _outHealth > 0 || m_explosionHistory.GetOrPrevious(_time) < lastAliveTime != true;
        }

        private bool IsQuit(double _time)
        {
            return m_quitTime <= _time;
        }

        public class ImmediateQuery
        {
            private readonly ActionHistory m_history;

            public ImmediateQuery(ActionHistory _history)
            {
                m_history = _history;
            }

            public bool CanKaze(double _time)
            {
                m_history.GetLifeInfo(_time, out _, out _, out bool canKaze);
                return canKaze;
            }

            public bool CanShootRifle(double _time, out float _power)
            {
                return m_history.CouldShootRifle(_time, out _power) && IsAlive(_time);
            }

            public bool CanShootRifle(double _time)
            {
                return CanShootRifle(_time, out _);
            }

            public bool CanShootRocket(double _time)
            {
                return m_history.CouldShootRocket(_time) && IsAlive(_time);
            }

            public int GetDeaths(double _time)
            {
                return m_history.GetDeaths(_time);
            }

            public int GetHealth(double _time)
            {
                return m_history.GetHealth(_time);
            }

            public int GetKills(double _time)
            {
                return m_history.GetKills(_time);
            }

            public bool IsAlive(double _time)
            {
                return GetHealth(_time) > 0 && !IsQuit(_time);
            }

            public bool IsAlive(double _time, out bool _shouldSpawn)
            {
                m_history.GetLifeInfo(_time, out int health, out _shouldSpawn, out _);
                return health > 0;
            }

            public bool IsQuit(double _time)
            {
                return m_history.IsQuit(_time);
            }

            public bool ShouldSpawn(double _time)
            {
                m_history.GetLifeInfo(_time, out _, out bool shouldSpawn, out _);
                return shouldSpawn;
            }
        }

        public class StaticQuery
        {
            public StaticQuery(ActionHistory _history, double _time)
            {
                IsQuit = _history.IsQuit(_time);
                _history.GetLifeInfo(_time, out int health, out bool shouldSpawn, out bool canKaze);
                Health = health;
                ShouldSpawn = shouldSpawn && !IsQuit;
                CanKaze = canKaze && !IsQuit;
                Kills = _history.GetKills(_time);
                Deaths = _history.GetDeaths(_time);
                CanShootRifle = _history.CouldShootRifle(_time, out float riflePower) && IsAlive;
                RiflePower = riflePower;
                CanShootRocket = _history.CouldShootRocket(_time) && IsAlive;
            }

            public bool CanKaze { get; }
            public bool CanShootRifle { get; }
            public bool CanShootRocket { get; }
            public int Deaths { get; }
            public int Health { get; }
            public bool IsAlive => Health > 0 && !IsQuit;
            public bool IsQuit { get; }
            public int Kills { get; }
            public float RiflePower { get; }
            public bool ShouldSpawn { get; }
        }

        #endregion Query
    }
}