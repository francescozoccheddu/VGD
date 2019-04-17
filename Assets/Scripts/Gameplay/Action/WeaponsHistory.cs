using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Action
{


    internal sealed class WeaponsHistory
    {

        public const double c_riflePowerUpTime = 2.0;
        public const double c_rifleCooldownTime = 0.4;
        public const float c_rifleMinPower = 0.25f;
        public const double c_rocketCooldownTime = 1.5;
        public const double c_spawnRocketTime = 1.0;
        public const double c_spawnRifleTime = 0.0;

        private sealed class ElapsedTimeHistory
        {

            private readonly LinkedListSimpleHistory<double> m_history;

            public ElapsedTimeHistory()
            {
                m_history = new LinkedListSimpleHistory<double>();
            }

            public void Put(double _time)
            {
                m_history.Set(_time);
            }

            public double? GetElapsedTime(double _time)
            {
                return _time - m_history.GetOrPrevious(_time);
            }

            public void Trim(double _time)
            {
                m_history.ForgetOlder(_time, true);
            }

        }

        private readonly ElapsedTimeHistory m_rifleHistory;
        private readonly ElapsedTimeHistory m_rocketHistory;
        private readonly ElapsedTimeHistory m_spawnHistory;

        public WeaponsHistory()
        {
            m_rifleHistory = new ElapsedTimeHistory();
            m_rocketHistory = new ElapsedTimeHistory();
            m_spawnHistory = new ElapsedTimeHistory();
        }

        public void PutRifleShot(double _time)
        {
            m_rifleHistory.Put(_time);
        }

        public void PutRocketShot(double _time)
        {
            m_rocketHistory.Put(_time);
        }

        public void PutSpawn(double _time)
        {
            m_spawnHistory.Put(_time);
        }

        private static bool CouldShootRifle(double _elapsedTime, out float _outPower)
        {
            bool canShoot = _elapsedTime >= c_rifleCooldownTime;
            if (canShoot)
            {
                float progress = (float) ((_elapsedTime - c_rifleCooldownTime) / c_riflePowerUpTime);
                _outPower = canShoot ? Mathf.Lerp(c_rifleMinPower, 1.0f, progress) : 0.0f;
            }
            else
            {
                _outPower = 0.0f;
            }
            return canShoot;
        }

        private static bool CouldShootRocket(double _elapsedTime)
        {
            return _elapsedTime >= c_rocketCooldownTime;
        }

        private double GetElapsedTime(double _time, ElapsedTimeHistory _weaponHistory, double _spawnWeaponTime)
        {
            double sinceSpawn = m_spawnHistory.GetElapsedTime(_time) ?? double.PositiveInfinity;
            double sinceShot = _weaponHistory.GetElapsedTime(_time) ?? double.PositiveInfinity;
            if (sinceShot <= sinceSpawn)
            {
                return sinceShot;
            }
            else
            {
                return sinceSpawn + _spawnWeaponTime;
            }
        }

        public bool CanShootRifle(double _time, out float _outPower)
        {
            return CouldShootRifle(GetElapsedTime(_time, m_rifleHistory, c_spawnRifleTime), out _outPower);
        }

        public bool CanShootRocket(double _time)
        {
            return CouldShootRocket(GetElapsedTime(_time, m_rocketHistory, c_spawnRocketTime));
        }

        public void Trim(double _time)
        {
            m_spawnHistory.Trim(_time);
            m_rocketHistory.Trim(_time);
            m_rifleHistory.Trim(_time);
        }

    }

}
