using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.PlayerView;
using Wheeled.UI.HUD;

namespace Wheeled.Gameplay.Offense
{
    public sealed class OffenseStage : EventHistory<ShotOffense>.ITarget
    {
        private class PendingRocketShot
        {
            public bool IsGone { get; private set; }

            private readonly RocketShotOffense m_offense;
            private readonly double m_time;
            private RocketBehaviour m_behaviour;

            public PendingRocketShot(double _time, RocketShotOffense _offense)
            {
                m_offense = _offense;
                m_time = _time;
            }

            public void Update(double _time)
            {
                double elapsed = _time - m_time;
                Vector3 origin = GetOrigin();
                Vector3 end = GetEnd();
                if (m_behaviour == null)
                {
                    
                    m_behaviour = Object.Instantiate(Scripts.Actors.rocketProjectile, origin, Quaternion.FromToRotation(origin, end)).GetComponent<RocketBehaviour>();
                    m_behaviour.SetColor(GameManager.Current.GetPlayerById(m_offense.OffenderId).GetColor());
                }
                float progress = (float) (RocketShotOffense.c_velocity * elapsed / Vector3.Distance(origin, end));
                Vector3 position = Vector3.Lerp(origin, end, progress);
                m_behaviour.Move(position);
                if (progress > 1.0f)
                {
                    if (m_offense.Hit != null)
                    {
                        m_behaviour.Explode(position);
                    }
                    IsGone = true;
                    m_behaviour.Dissolve();
                }
            }

            private Vector3 GetEnd()
            {
                return m_offense.Hit ?? (GetOrigin() + m_offense.Sight.Direction * (float) (RocketShotOffense.c_velocity * RocketShotOffense.c_maxLifetime));
            }

            private Vector3 GetOrigin()
            {
                return SocketsManagerBehaviour.Instance.rocket.GetPosition(m_offense.Origin, m_offense.Sight);
            }

            private Vector3 GetPosition(double _elapsedTime)
            {
                return GetOrigin() + m_offense.Sight.Direction * (float) (_elapsedTime * RocketShotOffense.c_velocity);
            }
        }

        private readonly EventHistory<ShotOffense> m_history;
        private readonly List<PendingRocketShot> m_pendingRocketShots;

        public OffenseStage()
        {
            m_history = new EventHistory<ShotOffense>()
            {
                Target = this
            };
            m_pendingRocketShots = new List<PendingRocketShot>();
        }

        public void Put(double _time, in LaserShotOffense _offense)
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

                case LaserShotOffense o:
                {
                    LaserBehaviour behaviour = Object.Instantiate(Scripts.Actors.rifleProjectile).GetComponent<LaserBehaviour>();
                    Vector3 origin = SocketsManagerBehaviour.Instance.rifle.GetPosition(o.Origin, o.Sight);
                    Vector3 end = o.Hit ?? (o.Origin + o.Sight.Direction * LaserShotOffense.c_maxDistance);
                    behaviour.SetColor(GameManager.Current.GetPlayerById(o.OffenderId).GetColor());
                    behaviour.Shoot(origin, end, o.Hit != null);
                }
                break;
            }
        }
    }
}