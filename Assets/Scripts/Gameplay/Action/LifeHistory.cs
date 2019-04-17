using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{
    internal class LifeHistory
    {

        public const int c_fullHealth = 100;
        public const int c_explosionHealth = -50;

        private readonly LinkedListHistory<double, DamageInfo> m_damages;
        private readonly LinkedListHistory<double, int> m_health;

        public LifeHistory()
        {
            m_damages = new LinkedListHistory<double, DamageInfo>();
            m_health = new LinkedListHistory<double, int>();
        }

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

        public struct KillInfo
        {
            public double time;
            public byte? offenderId;
        }

        public KillInfo? GetDeathInfo(double _time, out OffenseType _outOffenseType)
        {
            throw new System.NotImplementedException();
        }

        public KillInfo? GetExplosionInfo(double _time)
        {
            throw new System.NotImplementedException();
        }

    }
}
