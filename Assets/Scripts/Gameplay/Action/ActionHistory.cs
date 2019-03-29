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

        private struct Stats
        {
            public int kills;
            public int deaths;
        }

        private enum StatsEventType
        {
            Death, Kill
        }

        private readonly LinkedListHistory<double, int> m_healthHistory = new LinkedListHistory<double, int>();
        private readonly LinkedListHistory<double, Stats> m_statsHistory = new LinkedListHistory<double, Stats>();
        private readonly LinkedListHistory<double, StatsEventType> m_statsEventsHistory = new LinkedListHistory<double, StatsEventType>();
        private readonly LinkedListHistory<double, object> m_rifleShootHistory = new LinkedListHistory<double, object>();
        private readonly LinkedListHistory<double, object> m_rocketShootHistory = new LinkedListHistory<double, object>();

        public int Kills { get; private set; }
        public int Deaths { get; private set; }
        public int Health { get; private set; }
        public bool ShouldSpawn { get; private set; }
        public bool CanShootRocket { get; private set; }
        public bool CanShootRifle { get; private set; }
        public float RiflePower { get; private set; }
        public bool IsAlive => Health > 0;

        public void PutStats(double _time, int _kills, int _deaths)
        {
            m_statsHistory.Set(_time, new Stats { kills = _kills, deaths = _deaths });
        }

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
                m_statsEventsHistory.Set(_time, StatsEventType.Death);
            }
        }

        public void PutKill(double _time)
        {
            m_statsEventsHistory.Set(_time, StatsEventType.Kill);
        }

        public void PutRifleShot(double _time)
        {
            m_rifleShootHistory.Add(_time, null);
        }

        public void PutRocketShot(double _time)
        {
            m_rocketShootHistory.Add(_time, null);
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
                            Health = node.entry;
                            found = true;
                        }
                        if (node.entry == 0)
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
                        willSpawn |= node.entry > 0;
                    }
                }
                ShouldSpawn = Health == 0 && (_time - lastTime) >= c_respawnWaitTime && !willSpawn;
            }
            // Stats
            foreach (HistoryNode<double, Stats> node in m_statsHistory.GetReversedSequenceSince(_time, false, true))
            {
                Kills = node.entry.kills;
                Deaths = node.entry.deaths;
                foreach (HistoryNode<double, StatsEventType> eventNode in m_statsEventsHistory.GetReversedSequenceSince(_time, false, true))
                {
                    if (eventNode.time <= node.time)
                    {
                        break;
                    }
                    switch (eventNode.entry)
                    {
                        case StatsEventType.Death:
                        Deaths++;
                        break;
                        case StatsEventType.Kill:
                        Kills++;
                        break;
                    }
                }
                break;
            }
            // Shoots
            CanShootRocket = true;
            foreach (HistoryNode<double, object> node in m_rocketShootHistory.GetReversedSequenceSince(_time, false, true))
            {
                double elapsed = _time - node.time;
                CanShootRocket = elapsed >= c_rocketCooldown;
                break;
            }
            CanShootRifle = true;
            RiflePower = 1.0f;
            foreach (HistoryNode<double, object> node in m_rifleShootHistory.GetReversedSequenceSince(_time, false, true))
            {
                double elapsed = _time - node.time;
                CanShootRifle = elapsed >= c_rifleCooldown;
                RiflePower = Mathf.Clamp01((float) (elapsed / c_riflePowerUpTime));
                break;
            }
        }

        public void Trim(double _time)
        {
            m_healthHistory.ForgetOlder(_time, true);
            m_rifleShootHistory.ForgetOlder(_time, true);
            m_rocketShootHistory.ForgetOlder(_time, true);
            m_statsEventsHistory.ForgetOlder(_time, true);
            m_statsHistory.ForgetOlder(_time, true);
        }

    }

}
