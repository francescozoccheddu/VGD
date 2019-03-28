using System.Collections.Generic;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{

    internal interface IAction { }

    internal interface IHealthAction : IAction
    {

        int Health { get; }

    }

    internal struct HitAction : IAction
    {

    }

    internal struct SpawnAction : IHealthAction
    {
        public const int c_spawnHealth = 100;

        public int Health => c_spawnHealth;
    }

    internal struct RocketShootAction : IAction
    {

    }

    internal struct RifleShootAction : IAction
    {

    }

    internal struct DieAction : IHealthAction
    {
        public int Health => 0;
    }

    internal struct LifeUpdateAction : IHealthAction
    {
        public int Health { get; set; }
    }

    internal sealed class ActionHistory
    {

        private LinkedListHistory<double, IAction> m_actionHistory = new LinkedListHistory<double, IAction>();
        private LinkedListHistory<double, IHealthAction> m_healthHistory = new LinkedListHistory<double, IHealthAction>();
        private LinkedListHistory<double, RocketShootAction> m_rocketHistory = new LinkedListHistory<double, RocketShootAction>();
        private LinkedListHistory<double, RifleShootAction> m_rifleHistory = new LinkedListHistory<double, RifleShootAction>();

        public void Put(double _time, IAction _action)
        {
            m_actionHistory.Add(_time, _action);
            if (_action is IHealthAction healthAction)
            {
                m_healthHistory.Set(_time, healthAction);
            }
        }

        public double GetTimeSinceLastRifleShot(double _time)
        {
            foreach (HistoryNode<double, RifleShootAction> node in m_rifleHistory.GetReversedSequenceSince(_time, false, true))
            {
                return _time - node.time;
            }
            return double.PositiveInfinity;
        }

        public double GetTimeSinceLastRocketShot(double _time)
        {
            foreach (HistoryNode<double, RocketShootAction> node in m_rocketHistory.GetReversedSequenceSince(_time, false, true))
            {
                return _time - node.time;
            }
            return double.PositiveInfinity;
        }

        public bool IsAlive(double _time)
        {
            foreach (HistoryNode<double, IHealthAction> node in m_healthHistory.GetReversedSequenceSince(_time, true, false))
            {
                return node.entry.Health > 0;
            }
            return false;
        }

        public bool IsSpawnScheduled(double _time)
        {
            foreach (HistoryNode<double, IHealthAction> node in m_healthHistory.GetSequenceSince(_time, false, true))
            {
                if (node.entry is SpawnAction)
                {
                    return true;
                }
            }
            return false;
        }

        public void GetSpawnState(double _time, out bool _outIsAlive, out double _outTimeSinceLastState)
        {
            bool found = false;
            _outIsAlive = false;
            _outTimeSinceLastState = double.PositiveInfinity;
            foreach (HistoryNode<double, IHealthAction> node in m_healthHistory.GetReversedSequenceSince(_time, false, true))
            {
                if (!found || node.entry.Health > 0 == _outIsAlive)
                {
                    found = true;
                    _outIsAlive = node.entry.Health > 0;
                    _outTimeSinceLastState = _time - node.time;
                }
                else
                {
                    break;
                }
            }
        }

        public IEnumerable<HistoryNode<double, IAction>> GetActions(double _from, double _toInclusive, bool _includeFrom)
        {
            foreach (HistoryNode<double, IAction> node in m_actionHistory.GetSequenceSince(_from, false, true))
            {
                if (node.time > _toInclusive)
                {
                    break;
                }
                if (_includeFrom || node.time > _from)
                {
                    yield return node;
                }
            }
        }

        public void Trim(double _time)
        {
            m_healthHistory.ForgetOlder(_time, true);
            m_rocketHistory.ForgetOlder(_time, true);
            m_rifleHistory.ForgetOlder(_time, true);
            m_actionHistory.ForgetOlder(_time, true);
        }

    }

}
