using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.PlayerView;

namespace Wheeled.Gameplay.Stage
{
    internal sealed class OffenseBackstage
    {
        #region Public Interfaces

        public interface IValidationTarget
        {
            #region Public Methods

            IEnumerable<HitTarget> ProvideHitTarget(double _time, Offense _offense);

            void Damage(double _time, byte _offendedId, Offense _offense, float _damage);

            bool ShouldProcess(double _time, Offense _offense);

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Private Classes

        private abstract class PendingOffense
        {
            #region Public Properties

            public Offense Offense { get; }

            public bool IsGone { get; private set; }

            public byte OffenderId { get; }

            public double Time { get; }

            #endregion Public Properties

            #region Public Constructors

            public PendingOffense(double _time, Offense _offense)
            {
                IsGone = false;
                Time = _time;
                Offense = _offense;
            }

            #endregion Public Constructors

            #region Public Methods

            public abstract void Update(double _time, OffenseBackstage _stage);

            public void Dispose()
            {
                IsGone = true;
            }

            #endregion Public Methods
        }

        private class PendingExplosionOffense : PendingOffense
        {
            #region Private Fields

            private const float c_fullDamage = 1.0f;
            private const float c_innerRadius = 2.0f;
            private const float c_outerRadius = 5.0f;

            #endregion Private Fields

            #region Public Constructors

            public PendingExplosionOffense(double _time, ExplosionOffense _offense) : base(_time, _offense)
            {
            }

            #endregion Public Constructors

            #region Public Methods

            public override void Update(double _time, OffenseBackstage _stage)
            {
                if (_time >= Time)
                {
                    IEnumerable<HitTarget> targets = _stage.ValidationTarget?.ProvideHitTarget(Time, Offense);
                    if (targets != null)
                    {
                        foreach (HitTarget t in targets)
                        {
                            float damage;
                            float distance = Vector3.Distance(t.snapshot.simulation.Position, Offense.Origin);
                            if (distance <= c_innerRadius)
                            {
                                damage = c_fullDamage;
                            }
                            else if (distance <= c_outerRadius)
                            {
                                damage = Mathf.Clamp01((distance - c_innerRadius) / (c_outerRadius - c_innerRadius)) * c_fullDamage;
                            }
                            else
                            {
                                continue;
                            }
                            _stage.ValidationTarget.Damage(Time, t.playerId, Offense, damage);
                        }
                    }
                    Dispose();
                }
            }

            #endregion Public Methods
        }

        private sealed class PendingRifleShotOffense : PendingShotOffense
        {
            #region Private Fields

            private const float c_criticalDamage = 2.0f;
            private const float c_damage = 0.7f;

            #endregion Private Fields

            #region Public Constructors

            public PendingRifleShotOffense(double _time, RifleShotOffense _offense) : base(_time, _offense)
            {
            }

            #endregion Public Constructors

            #region Protected Methods

            protected override void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target)
            {
                if (_time >= Time)
                {
                    RifleShotOffense offense = (RifleShotOffense) Offense;
                    _probes.Clear();
                    {
                        IEnumerable<HitTarget> targets = _target?.ProvideHitTarget(_time, Offense);
                        if (targets != null)
                        {
                            foreach (HitTarget t in targets)
                            {
                                _probes.Add(t.playerId, t.snapshot);
                            }
                        }
                        Physics.SyncTransforms();
                    }
                    Vector3 origin = GetOrigin();
                    Vector3 end = origin + offense.Sight.Direction * RifleShotOffense.c_maxDistance;
                    if (_probes.RayCast(origin, end, out HitProbePool.HitInfo hitInfo))
                    {
                        end = hitInfo.position;
                        if (hitInfo.playerId != null)
                        {
                            float damage = (hitInfo.isCritical ? c_criticalDamage : c_damage) * offense.Power;
                            _target.Damage(Time, hitInfo.playerId.Value, offense, damage);
                        }
                        ((RifleShotOffense) Offense).Hit = hitInfo.position;
                    }
                    _probes.Clear();
                    Dispose();
                }
            }

            #endregion Protected Methods
        }

        private sealed class PendingRocketShotOffense : PendingShotOffense
        {
            #region Private Fields

            private const float c_fullDamage = 1.0f;
            private const double c_hitTestDuration = 0.5;
            private const float c_innerRadius = 2.0f;
            private const float c_outerRadius = 5.0f;
            private double m_lifetime;

            #endregion Private Fields

            #region Public Constructors

            public PendingRocketShotOffense(double _time, RocketShotOffense _offense) : base(_time, _offense)
            {
            }

            #endregion Public Constructors

            #region Protected Methods

            protected override void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target)
            {
                if (_time >= Time)
                {
                    double targetLifetime = Math.Min(_time - Time, RocketShotOffense.c_maxLifetime);
                    Vector3 position = GetPosition(m_lifetime);
                    while (m_lifetime < targetLifetime)
                    {
                        double nextLifeTime = Math.Min(m_lifetime + c_hitTestDuration, targetLifetime);
                        _probes.Clear();
                        {
                            IEnumerable<HitTarget> targets = _target?.ProvideHitTarget(m_lifetime + Time, Offense);
                            if (targets != null)
                            {
                                foreach (HitTarget t in targets)
                                {
                                    _probes.Add(t.playerId, t.snapshot);
                                }
                            }
                            Physics.SyncTransforms();
                        }
                        Vector3 nextPosition = GetPosition(nextLifeTime);
                        if (_probes.RayCast(position, nextPosition, out HitProbePool.HitInfo hitInfo))
                        {
                            float hitDistance = Vector3.Distance(hitInfo.position, GetOrigin());
                            double hitTime = Time + hitDistance / RocketShotOffense.c_velocity;
                            position = hitInfo.position;
                            IEnumerable<HitTarget> targets = _target?.ProvideHitTarget(hitTime, Offense);
                            if (targets != null)
                            {
                                foreach (HitTarget t in targets)
                                {
                                    float damage;
                                    float distance = Vector3.Distance(t.snapshot.simulation.Position, position);
                                    if (distance <= c_innerRadius)
                                    {
                                        damage = c_fullDamage;
                                    }
                                    else if (distance <= c_outerRadius)
                                    {
                                        damage = Mathf.Clamp01((distance - c_innerRadius) / (c_outerRadius - c_innerRadius)) * c_fullDamage;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    _target.Damage(hitTime, t.playerId, Offense, damage);
                                }
                            }
                            ((RocketShotOffense) Offense).Hit = hitInfo.position;
                            Dispose();
                            break;
                        }
                        else
                        {
                            m_lifetime = nextLifeTime;
                            position = nextPosition;
                        }
                    }
                    _probes.Clear();
                    if (m_lifetime >= RocketShotOffense.c_maxLifetime)
                    {
                        Dispose();
                    }
                }
            }

            #endregion Protected Methods

            #region Private Methods

            private Vector3 GetPosition(double _elapsedTime)
            {
                RocketShotOffense offense = (RocketShotOffense) Offense;
                return GetOrigin() + offense.Sight.Direction * (float) (_elapsedTime * RocketShotOffense.c_velocity);
            }

            #endregion Private Methods
        }

        private abstract class PendingShotOffense : PendingOffense
        {
            #region Public Constructors

            public PendingShotOffense(double _time, ShotOffense _offense) : base(_time, _offense)
            {
            }

            #endregion Public Constructors

            #region Public Methods

            public Vector3 GetOrigin()
            {
                ShotOffense offense = (ShotOffense) Offense;
                return SocketsManagerBehaviour.Instance.eye.GetPosition(offense.Origin, offense.Sight);
            }

            public override void Update(double _time, OffenseBackstage _stage)
            {
                if (!IsGone)
                {
                    UpdateTrajectory(_time, _stage.m_probePool, _stage.ValidationTarget);
                }
            }

            #endregion Public Methods

            #region Protected Methods

            protected abstract void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target);

            #endregion Protected Methods
        }

        #endregion Private Classes

        #region Public Structs

        public struct HitTarget
        {
            #region Public Fields

            public byte playerId;
            public Snapshot snapshot;

            #endregion Public Fields
        }

        #endregion Public Structs

        #region Public Properties

        public IValidationTarget ValidationTarget { get; set; }

        #endregion Public Properties

        #region Private Fields

        private readonly List<PendingOffense> m_offenses;
        private readonly HitProbePool m_probePool;

        #endregion Private Fields

        #region Public Constructors

        public OffenseBackstage()
        {
            m_offenses = new List<PendingOffense>();
            m_probePool = new HitProbePool();
        }

        #endregion Public Constructors

        #region Public Methods

        public void PutExplosion(double _time, ExplosionOffense _offense)
        {
            m_offenses.Add(new PendingExplosionOffense(_time, _offense));
        }

        public void PutRifle(double _time, RifleShotOffense _offense)
        {
            m_offenses.Add(new PendingRifleShotOffense(_time, _offense));
        }

        public void PutRocket(double _time, RocketShotOffense _offense)
        {
            m_offenses.Add(new PendingRocketShotOffense(_time, _offense));
        }

        public void UpdateUntil(double _time)
        {
            if (ValidationTarget != null)
            {
                foreach (PendingOffense o in m_offenses)
                {
                    if (ValidationTarget.ShouldProcess(o.Time, o.Offense))
                    {
                        o.Update(_time, this);
                    }
                    else
                    {
                        o.Dispose();
                    }
                }
            }
            m_offenses.RemoveAll(_p => _p.IsGone);
        }

        #endregion Public Methods
    }
}