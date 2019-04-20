using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{
    internal interface IReadOnlyLifeHistory
    {
        #region Public Methods

        int GetHealth(double _time);

        void GetLastDeathInfo(double _time, out DamageNode? _outDeath, out DamageNode? _outExplosion);

        #endregion Public Methods
    }

    internal static class LifeHistoryHelper
    {
        #region Public Methods

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

        #endregion Private Fields

        #region Public Constructors

        public LifeHistory()
        {
            m_damages = new LinkedListHistory<double, DamageInfo>();
            m_health = new LinkedListHistory<double, int>();
        }

        #endregion Public Constructors

        #region Public Methods

        public DamageInfo PutDamage(double _time, int _damage, byte _offenderId, OffenseType _offenseType)
        {
            return new DamageInfo
            {
                damage = _damage,
                maxHealth = GetHealth(_time) - _damage,
                offenderId = _offenderId,
                offenseType = _offenseType
            };
        }

        public void PutDamage(double _time, DamageInfo _info)
        {
            m_damages.Add(_time, _info);
        }

        public void PutHealth(double _time, int _health)
        {
            m_health.Set(_time, _health);
        }

        public int GetHealth(double _time)
        {
            HistoryNode<double, int>? healthNode = m_health.GetOrPrevious(_time);
            if (healthNode == null)
            {
                return 0;
            }
            int health = healthNode.Value.value;
            foreach (HistoryNode<double, DamageInfo> damageNode in m_damages
                .GetSequenceSince(healthNode.Value.time, false, true)
                .TakeWhile(_n => _n.time <= _time))
            {
                health = Mathf.Min(damageNode.value.maxHealth, health - damageNode.value.damage);
            }
            return health;
        }

        public void Trim(double _time)
        {
            m_health.ForgetOlder(_time, true);
            m_damages.ForgetOlder(_time, true);
        }

        public void GetLastDeathInfo(double _time, out DamageNode? _outDeath, out DamageNode? _outExplosion)
        {
            _outDeath = null;
            _outExplosion = null;
            HistoryNode<double, int>? healthNode = m_health
                .GetReversedSequenceSince(_time, false, true)
                .Where(_n => _n.value > 0)
                .Cast<HistoryNode<double, int>?>()
                .FirstOrDefault();
            if (healthNode == null)
            {
                return;
            }
            int health = healthNode.Value.value;
            int lastHealth = health;
            foreach (HistoryNode<double, DamageInfo> damageNode in m_damages
                .GetSequenceSince(healthNode.Value.time, false, true)
                .TakeWhile(_n => _n.time <= _time))
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
    }

    internal struct DamageNode
    {
        #region Public Fields

        public double time;
        public DamageInfo damage;

        #endregion Public Fields
    }
}