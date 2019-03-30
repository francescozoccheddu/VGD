using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{

    internal sealed class ActionHistory
    {

        private const float c_rifleCooldown = 1.0f;
        private const float c_rocketCooldown = 1.0f;
        private const float c_respawnWaitTime = 2.0f;
        private const float c_riflePowerUpTime = 3.0f;
        private const int c_fullHealth = 100;

        private readonly LinkedListHistory<double, int> m_healthHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListHistory<double, int> m_killCountHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListHistory<double, int> m_deathCountHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListSimpleHistory<double> m_rifleShootHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListSimpleHistory<double> m_rocketShootHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListSimpleHistory<double> m_killsHistory = new LinkedListSimpleHistory<double>();
        private readonly LinkedListSimpleHistory<double> m_deathsHistory = new LinkedListSimpleHistory<double>();
        private double? m_quitTime;

        public int Kills { get; private set; }
        public int Deaths { get; private set; }
        public int Health { get; private set; }
        public bool ShouldSpawn { get; private set; }
        public bool CanShootRocket { get; private set; }
        public bool CanShootRifle { get; private set; }
        public float RiflePower { get; private set; }
        public bool IsAlive => Health > 0;
        public bool IsQuit { get; private set; }

        public void PutSpawn(double _time)
        {
            PutHealth(_time, c_fullHealth);
        }

        public void PutDeath(double _time)
        {
            PutHealth(_time, 0);
        }

        public void PutHealth(double _time, int _health)
        {
            m_healthHistory.Set(_time, _health);
            if (_health <= 0)
            {
                m_deathsHistory.Set(_time);
            }
        }

        public void PutKill(double _time)
        {
            m_killsHistory.Set(_time);
        }

        public void PutDeaths(double _time, int _deaths)
        {
            m_deathCountHistory.Set(_time, _deaths);
        }

        public void PutKills(double _time, int _kills)
        {
            m_killCountHistory.Set(_time, _kills);
        }

        public void PutRifleShot(double _time)
        {
            m_rifleShootHistory.Add(_time);
        }

        public void PutRocketShot(double _time)
        {
            m_rocketShootHistory.Add(_time);
        }

        public void PutQuit(double _time)
        {
            if (!(m_quitTime < _time))
            {
                m_quitTime = _time;
            }
        }

        public void Update(double _time)
        {
            // Life
            {
                Health = 0;
                double lastTime = 0.0;
                bool found = false;
                bool willSpawn = false;
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
                            break;
                        }
                    }
                    else
                    {
                        willSpawn |= node.value > 0;
                    }
                }
                ShouldSpawn = Health == 0 && (_time - lastTime) >= c_respawnWaitTime && !willSpawn;
            }
            // Stats
            {
                HistoryNode<double, int>? node = m_killCountHistory.GetOrPrevious(_time);
                Kills = node?.value ?? 0;
                foreach (double time in m_killsHistory.GetReversedSequenceSince(_time, false, true))
                {
                    if (node?.time >= time)
                    {
                        break;
                    }
                    Kills++;
                }
            }
            {
                HistoryNode<double, int>? node = m_deathCountHistory.GetOrPrevious(_time);
                Deaths = node?.value ?? 0;
                foreach (double time in m_deathsHistory.GetReversedSequenceSince(_time, false, true))
                {
                    if (node?.time >= time)
                    {
                        break;
                    }
                    Deaths++;
                }
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
            }
            // Quit
            IsQuit = m_quitTime <= _time;
        }

        public void Trim(double _time)
        {
            m_healthHistory.ForgetOlder(_time, true);
            m_rifleShootHistory.ForgetOlder(_time, true);
            m_rocketShootHistory.ForgetOlder(_time, true);
            m_killCountHistory.ForgetOlder(_time, true);
            m_deathCountHistory.ForgetOlder(_time, true);
        }

    }

}
