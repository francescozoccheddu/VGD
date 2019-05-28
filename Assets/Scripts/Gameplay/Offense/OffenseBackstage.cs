using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.PlayerView;

namespace Wheeled.Gameplay.Offense
{
    public sealed class OffenseBackstage
    {
        public interface IValidationTarget
        {
            IEnumerable<HitTarget> ProvideHitTarget(double _time, Action.Offense _offense);

            void Damage(double _time, int _offendedId, Action.Offense _offense, float _damage);

            bool ShouldProcess(double _time, Action.Offense _offense);
        }

        private abstract class PendingOffense
        {
            public Action.Offense Offense { get; }

            public bool IsGone { get; private set; }

            public int OffenderId { get; }

            public double Time { get; }

            public PendingOffense(double _time, Action.Offense _offense)
            {
                IsGone = false;
                Time = _time;
                Offense = _offense;
            }

            public abstract void Update(double _time, OffenseBackstage _stage);

            public void Dispose() => IsGone = true;
        }

        private class PendingExplosionOffense : PendingOffense
        {
            private const float c_fullDamage = 0.8f;
            private const float c_innerRadius = 1.0f;
            private const float c_outerRadius = 3.0f;

            public PendingExplosionOffense(double _time, ExplosionOffense _offense) : base(_time, _offense)
            {
            }

            public override void Update(double _time, OffenseBackstage _stage)
            {
                if (_time >= Time)
                {
                    IEnumerable<HitTarget> targets = _stage.ValidationTarget?.ProvideHitTarget(Time, Offense);
                    if (targets != null)
                    {
                        foreach (HitTarget t in targets)
                        {
                            float distance = Vector3.Distance(t.snapshot.simulation.Position, Offense.Origin);
                            float intensity = (1.0f - Mathf.Clamp01((distance - c_innerRadius) / (c_outerRadius - c_innerRadius)));
                            if (intensity > 0.0f)
                            {
                                _stage.ValidationTarget.Damage(Time, t.playerId, Offense, intensity * c_fullDamage);
                            }
                        }
                    }
                    Dispose();
                }
            }
        }

        private sealed class PendingLaserShotOffense : PendingShotOffense
        {
            private const float c_criticalFullDamage = 0.9f;
            private const float c_fullDamage = 0.3f;

            public PendingLaserShotOffense(double _time, LaserShotOffense _offense) : base(_time, _offense)
            {
            }

            protected override void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target)
            {
                if (_time >= Time)
                {
                    LaserShotOffense offense = (LaserShotOffense) Offense;
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
                    Vector3 end = origin + offense.Sight.Direction * LaserShotOffense.c_maxDistance;
                    if (_probes.RayCast(origin, end, out HitProbePool.HitInfo hitInfo))
                    {
                        end = hitInfo.position;
                        if (hitInfo.playerId != null)
                        {
                            float damage = (hitInfo.isCritical ? c_criticalFullDamage : c_fullDamage) * offense.Power;
                            _target.Damage(Time, hitInfo.playerId.Value, offense, damage);
                        }
                        ((LaserShotOffense) Offense).Hit = hitInfo.position;
                    }
                    _probes.Clear();
                    Dispose();
                }
            }
        }

        private sealed class PendingRocketShotOffense : PendingShotOffense
        {
            private const float c_fullDamage = 0.7f;
            private const double c_hitTestDuration = 0.5;
            private const float c_innerRadius = 1.0f;
            private const float c_outerRadius = 3.0f;
            private double m_lifetime;

            public PendingRocketShotOffense(double _time, RocketShotOffense _offense) : base(_time, _offense)
            {
            }

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
                                    float distance = Vector3.Distance(t.snapshot.simulation.Position, position);
                                    float intensity = (1.0f - Mathf.Clamp01((distance - c_innerRadius) / (c_outerRadius - c_innerRadius)));
                                    if (intensity > 0.0f)
                                    {
                                        _target.Damage(hitTime, t.playerId, Offense, intensity * c_fullDamage);
                                    }
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

            private Vector3 GetPosition(double _elapsedTime)
            {
                RocketShotOffense offense = (RocketShotOffense) Offense;
                return GetOrigin() + offense.Sight.Direction * (float) (_elapsedTime * RocketShotOffense.c_velocity);
            }
        }

        private abstract class PendingShotOffense : PendingOffense
        {
            public PendingShotOffense(double _time, ShotOffense _offense) : base(_time, _offense)
            {
            }

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

            protected abstract void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target);
        }

        public struct HitTarget
        {
            public int playerId;
            public Snapshot snapshot;
        }

        public IValidationTarget ValidationTarget { get; set; }

        private readonly List<PendingOffense> m_offenses;
        private readonly HitProbePool m_probePool;

        public OffenseBackstage()
        {
            m_offenses = new List<PendingOffense>();
            m_probePool = new HitProbePool();
        }

        public void PutExplosion(double _time, ExplosionOffense _offense) => m_offenses.Add(new PendingExplosionOffense(_time, _offense));

        public void PutRifle(double _time, LaserShotOffense _offense) => m_offenses.Add(new PendingLaserShotOffense(_time, _offense));

        public void PutRocket(double _time, RocketShotOffense _offense) => m_offenses.Add(new PendingRocketShotOffense(_time, _offense));

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
    }
}