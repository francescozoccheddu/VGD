using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Stage
{
    internal sealed class OffenseStage
    {
        private readonly List<Offense> m_offenses;
        private readonly HitProbePool m_probePool;

        public OffenseStage()
        {
            m_offenses = new List<Offense>();
            m_probePool = new HitProbePool();
        }

        public interface IValidationTarget
        {
            IEnumerable<HitTarget> GetHitTargets(double _time, byte _shooterId);

            void Offense(byte _offenderId, byte _offendedId, float _damage, OffenseType _type);
        }

        public IValidationTarget ValidationTarget { get; set; }

        public void Kaze(double _time, byte _id, Vector3 _position)
        {
            m_offenses.Add(new KazeOffense(_time, _id, _position));
        }

        public void ShootRifle(double _time, Vector3 _origin, Vector3 _direction, byte _shooterId, float _power)
        {
            m_offenses.Add(new RifleProjectile(_time, _shooterId, _origin, _direction, _power));
        }

        public void ShootRocket(double _time, Vector3 _origin, Vector3 _direction, byte _shooterId)
        {
            m_offenses.Add(new RocketProjectile(_time, _shooterId, _origin, _direction));
        }

        public void Update(double _time)
        {
            foreach (Offense o in m_offenses)
            {
                o.Update(_time, this);
            }
            m_offenses.RemoveAll(_p => _p.IsGone);
        }

        public struct HitTarget
        {
            public byte playerId;
            public Snapshot snapshot;
        }

        #region Offense

        private class KazeOffense : Offense
        {
            private const float c_fullDamage = 1.0f;
            private const float c_innerRadius = 2.0f;
            private const float c_outerRadius = 5.0f;

            private readonly Vector3 m_position;

            public KazeOffense(double _time, byte _offenderId, Vector3 _position) : base(_time, _offenderId)
            {
                m_position = _position;
            }

            public override void Update(double _time, OffenseStage _stage)
            {
                if (_time >= Time)
                {
                    IEnumerable<HitTarget> targets = _stage.ValidationTarget?.GetHitTargets(Time, OffenderId);
                    if (targets != null)
                    {
                        foreach (HitTarget t in targets)
                        {
                            float damage;
                            float distance = Vector3.Distance(t.snapshot.simulation.position, m_position);
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
                            _stage.ValidationTarget.Offense(OffenderId, t.playerId, damage, OffenseType.Kaze);
                        }
                    }
                    Dispose();
                }
            }
        }

        private abstract class Offense
        {
            public Offense(double _time, byte _offenderId)
            {
                IsGone = false;
                Time = _time;
                OffenderId = _offenderId;
            }

            public bool IsGone { get; private set; }

            public byte OffenderId { get; }

            public double Time { get; }

            public abstract void Update(double _time, OffenseStage _stage);

            protected void Dispose()
            {
                IsGone = true;
            }
        }

        private sealed class RifleProjectile : ShootOffense
        {
            private const float c_criticalDamage = 2.0f;
            private const float c_damage = 0.7f;
            private readonly float m_power;

            public RifleProjectile(double _shootTime, byte _shooterId, Vector3 _origin, Vector3 _direction, float _power) : base(_shootTime, _shooterId, _origin, _direction)
            {
                m_power = _power;
            }

            protected override void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target)
            {
                if (_time >= Time)
                {
                    _probes.Clear();
                    {
                        IEnumerable<HitTarget> targets = _target?.GetHitTargets(_time, OffenderId);
                        if (targets != null)
                        {
                            foreach (HitTarget t in targets)
                            {
                                _probes.Add(t.playerId, t.snapshot);
                            }
                        }
                    }
                    Vector3 end = m_origin + m_direction * 100;
                    bool hit = false;
                    if (_probes.RayCast(m_origin, end, out HitProbePool.HitInfo hitInfo))
                    {
                        end = hitInfo.position;
                        if (hitInfo.playerId != null)
                        {
                            float damage = (hitInfo.isCritical ? c_criticalDamage : c_damage) * m_power;
                            _target.Offense(OffenderId, hitInfo.playerId.Value, damage, OffenseType.Rifle);
                        }
                        hit = true;
                    }
                    _probes.Clear();
                    GameObject gameObject = UnityEngine.Object.Instantiate(ScriptManager.Actors.rifleProjectile);
                    gameObject.GetComponent<RifleProjectileBehaviour>()?.Shoot(m_origin, end, hit);
                    Dispose();
                }
            }
        }

        private sealed class RocketProjectile : ShootOffense
        {
            private const float c_fullDamage = 1.0f;
            private const double c_hitTestDuration = 0.5;
            private const float c_innerRadius = 2.0f;
            private const double c_maxLifetime = 5.0;
            private const float c_outerRadius = 5.0f;
            private const float c_velocity = 20.0f;
            private RocketProjectileBehaviour m_behaviour;
            private double m_lifetime;

            public RocketProjectile(double _shootTime, byte _shooterId, Vector3 _origin, Vector3 _direction) : base(_shootTime, _shooterId, _origin, _direction)
            {
                m_lifetime = 0.0;
            }

            protected override void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target)
            {
                if (_time >= Time)
                {
                    if (m_behaviour == null)
                    {
                        m_behaviour = UnityEngine.Object.Instantiate(ScriptManager.Actors.rocketProjectile).GetComponent<RocketProjectileBehaviour>();
                        m_behaviour.Shoot(m_origin, m_direction);
                    }
                    double targetLifetime = Math.Min(_time - Time, c_maxLifetime);
                    Vector3 position = GetPosition(m_lifetime);
                    while (m_lifetime < targetLifetime)
                    {
                        double nextLifeTime = Math.Min(m_lifetime + c_hitTestDuration, targetLifetime);
                        _probes.Clear();
                        {
                            IEnumerable<HitTarget> targets = _target?.GetHitTargets(m_lifetime + Time, OffenderId);
                            if (targets != null)
                            {
                                foreach (HitTarget t in targets)
                                {
                                    _probes.Add(t.playerId, t.snapshot);
                                }
                            }
                        }
                        Vector3 nextPosition = GetPosition(nextLifeTime);
                        if (_probes.RayCast(position, nextPosition, out HitProbePool.HitInfo hitInfo))
                        {
                            position = hitInfo.position;
                            m_behaviour?.Explode(position);
                            IEnumerable<HitTarget> targets = _target?.GetHitTargets(m_lifetime + Time, OffenderId);
                            if (targets != null)
                            {
                                foreach (HitTarget t in targets)
                                {
                                    float damage;
                                    float distance = Vector3.Distance(t.snapshot.simulation.position, position);
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
                                    _target.Offense(OffenderId, t.playerId, damage, OffenseType.Rocket);
                                }
                            }
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
                    m_behaviour?.Move(position);
                    if (m_lifetime >= c_maxLifetime)
                    {
                        m_behaviour?.Dissolve();
                        Dispose();
                    }
                }
            }

            private Vector3 GetPosition(double _elapsedTime)
            {
                return m_origin + m_direction * (float) (_elapsedTime * c_velocity);
            }
        }

        private abstract class ShootOffense : Offense
        {
            protected readonly Vector3 m_direction;
            protected readonly Vector3 m_origin;

            protected ShootOffense(double _shootTime, byte _shooterId, Vector3 _origin, Vector3 _direction) : base(_shootTime, _shooterId)
            {
                m_origin = _origin;
                m_direction = _direction.normalized;
            }

            public override void Update(double _time, OffenseStage _stage)
            {
                if (!IsGone)
                {
                    UpdateTrajectory(_time, _stage.m_probePool, _stage.ValidationTarget);
                }
            }

            protected abstract void UpdateTrajectory(double _time, HitProbePool _probes, IValidationTarget _target);
        }

        #endregion Offense
    }
}