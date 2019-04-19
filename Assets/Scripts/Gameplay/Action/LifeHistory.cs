using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{
    internal interface IReadOnlyLifeHistory
    {
        #region Public Methods

        int GetHealth(double _time);

        void GetLastDeathInfo(out KillInfo? _outDeath, out KillInfo? _outExplosion);

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
                .GetSequenceSince(_time, false, true)
                .Where(_n => _n.time > healthNode.Value.time)
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

        public void GetLastDeathInfo(out KillInfo? _outDeath, out KillInfo? _outExplosion)
        {
            throw new System.NotImplementedException();
        }

        #endregion Public Methods
    }

    internal struct KillInfo
    {
        #region Public Fields

        public double time;
        public byte? offenderId;

        #endregion Public Fields
    }
}