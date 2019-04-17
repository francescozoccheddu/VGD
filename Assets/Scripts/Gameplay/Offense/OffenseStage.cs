using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;

namespace Wheeled.Gameplay.Stage
{
    internal sealed class OffenseStage : ActionHistory<Offense>.ITarget
    {

        private class PendingRocketShot
        {

            public PendingRocketShot(double _time, RocketShotOffense _offense)
            {
                m_offense = _offense;
                m_time = _time;
            }

            private readonly RocketShotOffense m_offense;
            private readonly double m_time;
            private readonly RocketProjectileBehaviour m_behaviour;

            public void Update(double _time)
            {
                double elapsed = _time - m_time;
                float distance = (float) (elapsed * OffenseBackstage.c_rocketShotVelocity);
                if (distance >= m_offense.HitDistance)
                {
                    distance = m_offense.HitDistance.Value;
                }
                Vector3 position = m_offense.Origin + m_offense.Direction * distance;
                m_behaviour.Move(position);
                if (distance >= m_offense.HitDistance)
                {
                    m_behaviour.Explode(position);
                }
                if (elapsed >= OffenseBackstage.c_maxRocketShotLifetime || distance >= m_offense.HitDistance)
                {
                    IsGone = true;
                    m_behaviour.Dissolve();
                }
            }

            private Vector3 GetPosition(double _elapsedTime)
            {
                return m_offense.Origin + m_offense.Direction * (float) (_elapsedTime * OffenseBackstage.c_rocketShotVelocity);
            }

            public bool IsGone { get; private set; }

        }

        private readonly ActionHistory<Offense> m_history;
        private readonly List<PendingRocketShot> m_pendingRocketShots;

        public OffenseStage()
        {
            m_history = new ActionHistory<Offense>();
            m_pendingRocketShots = new List<PendingRocketShot>();
        }

        public void Put(double _time, in RifleShotOffense _offense)
        {
            m_history.Put(_time, _offense);
        }

        public void Put(double _time, object _key, in RocketShotOffense _offense)
        {
            m_history.Put(_time, _offense);
        }

        public void Put(double _time, in ExplosionOffense _offense)
        {
            m_history.Put(_time, _offense);
        }

        public void Update(double _time)
        {
            m_history.PerformUntil(_time);
            foreach (PendingRocketShot o in m_pendingRocketShots)
            {
                o.Update(_time);
            }
            m_pendingRocketShots.RemoveAll(_o => _o.IsGone);
        }

        void ActionHistory<Offense>.ITarget.Perform(double _time, Offense _value)
        {
            switch (_value)
            {
                case RocketShotOffense o:
                m_pendingRocketShots.Add(new PendingRocketShot(_time, o));
                break;
                case RifleShotOffense o:
                {
                    RifleProjectileBehaviour behaviour = Object.Instantiate(ScriptManager.Actors.rifleProjectile).GetComponent<RifleProjectileBehaviour>();
                    Vector3 end = o.Origin + o.Direction * (o.HitDistance ?? 100);
                    behaviour.Shoot(o.Origin, end, o.HitDistance != null);
                }
                break;
                case ExplosionOffense o:
                Object.Instantiate(ScriptManager.Actors.explosion, o.Origin, Quaternion.identity);
                break;
            }
        }
    }
}