using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;

namespace Wheeled.Gameplay.Stage
{
    internal sealed class OffenseStage : EventHistory<ShotOffense>.ITarget
    {
        #region Private Classes

        private class PendingRocketShot
        {
            #region Public Properties

            public bool IsGone { get; private set; }

            #endregion Public Properties

            #region Private Fields

            private readonly RocketShotOffense m_offense;
            private readonly double m_time;
            private RocketProjectileBehaviour m_behaviour;

            #endregion Private Fields

            #region Public Constructors

            public PendingRocketShot(double _time, RocketShotOffense _offense)
            {
                m_offense = _offense;
                m_time = _time;
            }

            #endregion Public Constructors

            #region Public Methods

            public void Update(double _time)
            {
                if (m_behaviour == null)
                {
                    m_behaviour = Object.Instantiate(ScriptManager.Actors.rocketProjectile).GetComponent<RocketProjectileBehaviour>();
                }
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

            #endregion Public Methods

            #region Private Methods

            private Vector3 GetPosition(double _elapsedTime)
            {
                return m_offense.Origin + m_offense.Direction * (float) (_elapsedTime * OffenseBackstage.c_rocketShotVelocity);
            }

            #endregion Private Methods
        }

        #endregion Private Classes

        #region Private Fields

        private readonly EventHistory<ShotOffense> m_history;
        private readonly List<PendingRocketShot> m_pendingRocketShots;

        #endregion Private Fields

        #region Public Constructors

        public OffenseStage()
        {
            m_history = new EventHistory<ShotOffense>()
            {
                Target = this
            };
            m_pendingRocketShots = new List<PendingRocketShot>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Put(double _time, in RifleShotOffense _offense)
        {
            m_history.Put(_time, _offense);
        }

        public void Put(double _time, in RocketShotOffense _offense)
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

        void EventHistory<ShotOffense>.ITarget.Perform(double _time, ShotOffense _value)
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
            }
        }

        #endregion Public Methods
    }
}