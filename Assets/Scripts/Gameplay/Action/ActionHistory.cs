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
        private double? m_time;

        public interface ITarget
        {
            void PerformDamage(DamageInfo _info);

            void PerformDeath(DeathInfo _info);

            void PerformHitConfirm(HitConfirmInfo _info);

            void PerformShoot(ShotInfo _info);

            void PerformSpawn(SpawnInfo _info);
        }

        private interface IAction
        {
            void Perform(ITarget _target);
        }

        public bool CanKaze { get; private set; }
        public bool CanShootRifle { get; private set; }
        public bool CanShootRocket { get; private set; }
        public int Deaths { get; private set; }
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;
        public bool IsQuit { get; private set; }
        public int Kills { get; private set; }
        public float RiflePower { get; private set; }
        public bool ShouldSpawn { get; private set; }
        public ITarget Target { get; set; }

        public void Perform()
        {
            foreach ((double time, IAction action) in m_actionHistory.GetFullSequence().Where(_n => _n.time <= m_time))
            {
                action.Perform(Target);
            }
            if (m_time != null)
            {
                m_actionHistory.ForgetAndOlder(m_time.Value);
            }
        }

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
            m_actionHistory.Add(_time, new ShootAction { info = _info });
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

        public void Update(double _time)
        {
            m_time = _time;
            // Quit
            IsQuit = m_quitTime <= _time;
            if (!IsQuit)
            {
                // Life
                {
                    Health = 0;
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
                                Health = node.value;
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
                    ShouldSpawn = Health == 0 && (_time - lastTime) >= c_respawnWaitTime && !willSpawn;
                    CanKaze = IsAlive || m_explosionHistory.GetOrPrevious(_time) < lastAliveTime != true;
                }
                // Shoots
                {
                    double? lastRocketShot = m_rocketShootHistory.GetOrPrevious(_time);
                    if (lastRocketShot != null)
                    {
                        double elapsed = _time - lastRocketShot.Value;
                        CanShootRocket = elapsed >= c_rocketCooldown;
                    }
                    else
                    {
                        CanShootRocket = true;
                    }
                    CanShootRocket &= IsAlive;
                }
                {
                    double? lastRifleShot = m_rocketShootHistory.GetOrPrevious(_time);
                    if (lastRifleShot != null)
                    {
                        double elapsed = _time - lastRifleShot.Value;
                        CanShootRifle = elapsed >= c_rifleCooldown;
                        RiflePower = Mathf.Clamp01((float) (elapsed / c_riflePowerUpTime));
                    }
                    else
                    {
                        CanShootRifle = true;
                        RiflePower = 1.0f;
                    }
                    CanShootRifle &= IsAlive;
                }
            }
            else
            {
                CanKaze = false;
                CanShootRifle = false;
                CanShootRocket = false;
                RiflePower = 0.0f;
                Health = 0;
                ShouldSpawn = false;
            }
            // Stats
            {
                HistoryNode<double, int>? node = m_killCountHistory.GetOrPrevious(_time);
                Kills = node?.value ?? 0;
                foreach (double time in m_killsHistory.GetReversedSequenceSince(_time, false, true).Where(_n => node?.time >= _n != true))
                {
                    Kills++;
                }
            }
            {
                HistoryNode<double, int>? node = m_deathCountHistory.GetOrPrevious(_time);
                Deaths = node?.value ?? 0;
                IEnumerable<HistoryNode<double, int>> sequence = node != null ? m_healthHistory.GetSequenceSince(node.Value.time, true, true) : m_healthHistory.GetFullSequence();
                bool alive = false;
                foreach (HistoryNode<double, int> healthNode in sequence.Where(_n => _n.time <= _time))
                {
                    if (alive && healthNode.value <= 0 && node?.time >= healthNode.time != true)
                    {
                        Deaths++;
                    }
                    alive = healthNode.value > 0;
                }
            }
        }

        private struct DamageAction : IAction
        {
            public DamageInfo info;

            void IAction.Perform(ITarget _target)
            {
                _target?.PerformDamage(info);
            }
        }

        private struct DeathAction : IAction
        {
            public DeathInfo info;

            void IAction.Perform(ITarget _target)
            {
                _target?.PerformDeath(info);
            }
        }

        private struct HitConfirmAction : IAction
        {
            public HitConfirmInfo info;

            void IAction.Perform(ITarget _target)
            {
                _target?.PerformHitConfirm(info);
            }
        }

        private struct ShootAction : IAction
        {
            public ShotInfo info;

            void IAction.Perform(ITarget _target)
            {
                _target?.PerformShoot(info);
            }
        }

        private struct SpawnAction : IAction
        {
            public SpawnInfo info;

            void IAction.Perform(ITarget _target)
            {
                _target?.PerformSpawn(info);
            }
        }
    }
}