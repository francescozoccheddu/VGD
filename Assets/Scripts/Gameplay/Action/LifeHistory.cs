using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{
    internal interface IReadOnlyLifeHistory
    {
        #region Public Methods

        int? GetHealthOrNull(double _time);

        void GetLastDeathInfo(double _time, out DamageNode? _outDeath, out DamageNode? _outExplosion);

        #endregion Public Methods
    }

    internal static class LifeHistoryHelper
    {
        #region Public Methods

        public static ELifeState GetLifeState(int? _health)
        {
            if (_health == null)
            {
                return ELifeState.Unknown;
            }
            else if (IsExploded(_health.Value))
            {
                return ELifeState.Exploded;
            }
            else if (IsAlive(_health.Value))
            {
                return ELifeState.Alive;
            }
            else
            {
                return ELifeState.Dead;
            }
        }

        public static int GetHealth(this IReadOnlyLifeHistory _history, double _time)
        {
            return _history.GetHealthOrNull(_time) ?? 0;
        }

        public static bool IsAlive(int _health)
        {
            return _health > 0;
        }

        public static bool IsExploded(int _health)
        {
            return _health <= LifeHistory.c_explosionHealth;
        }

        public static bool IsAlive(this IReadOnlyLifeHistory _history, double _time)
        {
            return IsAlive(_history.GetHealth(_time));
        }

        public static bool IsExploded(this IReadOnlyLifeHistory _history, double _time)
        {
            return IsExploded(_history.GetHealth(_time));
        }

        public static ELifeState GetLifeState(this IReadOnlyLifeHistory _history, double _time)
        {
            return GetLifeState(_history.GetHealthOrNull(_time));
        }

        public static double? GetTimeSinceLastDeath(this IReadOnlyLifeHistory _history, double _time)
        {
            _history.GetLastDeathInfo(_time, out DamageNode? node, out _);
            return _time - node?.time;
        }

        #endregion Public Methods
    }

    internal class LifeHistory : IReadOnlyLifeHistory
    {
        #region Public Fields

        public const int c_fullHealth = 100;
        public const int c_explosionHealth = -50;

        #endregion Public Fields

        #region Private Fields

        private readonly LinkedListHistory<double, DamageInfo> m_damages;
        private readonly LinkedListHistory<double, int> m_health;
        private HistoryNode<double, int>? m_trimmedNode;

        #endregion Private Fields

        #region Public Constructors

        public LifeHistory()
        {
            m_damages = new LinkedListHistory<double, DamageInfo>();
            m_health = new LinkedListHistory<double, int>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void PutDamage(double _time, DamageInfo _info)
        {
            m_damages.Add(_time, _info);
        }

        public void PutHealth(double _time, int _health)
        {
            m_health.Set(_time, _health);
        }

        public int? GetHealthOrNull(double _time)
        {
            HistoryNode<double, int>? healthNode = GetLastHealthNode(_time);
            if (healthNode == null)
            {
                return null;
            }
            int health = healthNode.Value.value;
            foreach (HistoryNode<double, DamageInfo> damageNode in m_damages
                .Between(healthNode.Value.time, _time))
            {
                health = Mathf.Min(damageNode.value.maxHealth, health - damageNode.value.damage);
            }
            return health;
        }

        public void Trim(double _time)
        {
            int? health = GetHealthOrNull(_time);
            if (health != null)
            {
                m_trimmedNode = new HistoryNode<double, int>
                {
                    time = _time,
                    value = health.Value
                };
            }
            m_health.ForgetOlder(_time, true);
            m_damages.ForgetOlder(_time, true);
        }

        public void GetLastDeathInfo(double _time, out DamageNode? _outDeath, out DamageNode? _outExplosion)
        {
            _outDeath = null;
            _outExplosion = null;
            HistoryNode<double, int>? healthNode = m_health.Last(_time, _n => _n.value > 0);
            if (m_trimmedNode?.value > 0 && m_trimmedNode?.time > healthNode?.time)
            {
                healthNode = m_trimmedNode;
            }
            if (healthNode == null)
            {
                return;
            }
            int health = healthNode.Value.value;
            int lastHealth = health;
            foreach (HistoryNode<double, DamageInfo> damageNode in m_damages
                .Between(healthNode.Value.time, _time))
            {
                lastHealth = health;
                health = Mathf.Min(damageNode.value.maxHealth, health - damageNode.value.damage);
                if (LifeHistoryHelper.IsAlive(lastHealth) && !LifeHistoryHelper.IsAlive(health))
                {
                    _outDeath = new DamageNode
                    {
                        damage = damageNode.value,
                        time = damageNode.time
                    };
                }
                if (!LifeHistoryHelper.IsExploded(lastHealth) && LifeHistoryHelper.IsExploded(health))
                {
                    _outExplosion = new DamageNode
                    {
                        damage = damageNode.value,
                        time = damageNode.time
                    };
                    break;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private HistoryNode<double, int>? GetLastHealthNode(double _time)
        {
            HistoryNode<double, int>? fromHistory = m_health.Last(_time);
            if (fromHistory?.time < m_trimmedNode?.time != true)
            {
                return fromHistory;
            }
            else
            {
                return m_trimmedNode;
            }
        }

        #endregion Private Methods
    }

    internal struct DamageNode
    {
        #region Public Fields

        public double time;
        public DamageInfo damage;

        #endregion Public Fields
    }

    public enum ELifeState
    {
        Alive, Dead, Exploded, Unknown
    }
}