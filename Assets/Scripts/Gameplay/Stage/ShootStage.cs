﻿using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Stage
{
    internal sealed class ShootStage
    {
        private static readonly List<GameObject> s_probes = new List<GameObject>();
        private readonly List<Projectile> m_projectiles;

        public ShootStage()
        {
            m_projectiles = new List<Projectile>();
        }

        public interface IValidationTarget
        {
            IEnumerable<Snapshot> GetPlayersAt(double _time);

            void RifleHit(double _time, byte _id, Collider _collider, float _power);

            void RocketHit(double _time, byte _id, Collider _collider);
        }

        public IValidationTarget ValidationTarget { get; set; }

        public void ShootRifle(double _time, Vector3 _origin, Vector3 _direction, byte _shooterId, float _power)
        {
            m_projectiles.Add(new RifleProjectile(_direction, _origin, _time, _shooterId, _power));
        }

        public void ShootRocket(double _time, Vector3 _origin, Vector3 _direction, byte _shooterId)
        {
            m_projectiles.Add(new RocketProjectile(_direction, _origin, _time, _shooterId));
        }

        public void Update(double _time)
        {
            foreach (Projectile p in m_projectiles)
            {
                p.Update(_time, ValidationTarget);
            }
            m_projectiles.RemoveAll(_p => _p.IsGone);
        }

        private abstract class Projectile
        {
            protected readonly Vector3 m_direction;
            protected readonly Vector3 m_origin;
            protected readonly byte m_shooterId;
            protected readonly double m_shootTime;

            protected Projectile(Vector3 _direction, Vector3 _origin, double _shootTime, byte _shooterId)
            {
                m_direction = _direction;
                m_origin = _origin;
                m_shootTime = _shootTime;
                m_shooterId = _shooterId;
            }

            public bool IsGone { get; private set; }

            public void Update(double _time, IValidationTarget _target)
            {
                if (!IsGone)
                {
                    UpdateTrajectory(_time, _target);
                }
            }

            protected static bool Raycast(Vector3 _start, Vector3 _end, out Hit _outHit)
            {
                bool wasHit = Physics.Raycast(new Ray(_start, _end - _start), out RaycastHit hit, 500, ~0);
                _outHit = new Hit
                {
                    position = hit.point
                };
                return wasHit;
            }

            protected void Dispose()
            {
                IsGone = true;
            }

            protected abstract void UpdateTrajectory(double _time, IValidationTarget _target);

            protected struct Hit
            {
                public Collider collider;
                public Vector3 normal;
                public Vector3 position;
            }
        }

        private sealed class RifleProjectile : Projectile
        {
            private readonly float m_power;

            public RifleProjectile(Vector3 _direction, Vector3 _origin, double _shootTime, byte _shooterId, float _power) : base(_direction, _origin, _shootTime, _shooterId)
            {
                m_power = _power;
            }

            protected override void UpdateTrajectory(double _time, IValidationTarget _target)
            {
                if (_time >= m_shootTime)
                {
                    IEnumerable<Snapshot> snapshots = _target?.GetPlayersAt(_time);
                    if (snapshots != null)
                    {
                        int i = 0;
                        foreach (Snapshot snapshot in snapshots)
                        {
                            if (i >= s_probes.Count)
                            {
                                s_probes.Add(Object.Instantiate(ScriptManager.Actors.collisionProbe, snapshot.simulation.position, Quaternion.identity));
                            }
                            else if (s_probes[i] == null)
                            {
                                s_probes[i] = Object.Instantiate(ScriptManager.Actors.collisionProbe, snapshot.simulation.position, Quaternion.identity);
                            }
                            else
                            {
                                s_probes[i].transform.position = snapshot.simulation.position;
                            }
                            i++;
                        }
                    }
                    Vector3 end = m_origin + m_direction * 100;
                    if (Raycast(m_origin, end, out Hit hit))
                    {
                        end = hit.position;
                    }
                    GameObject gameObject = Object.Instantiate(ScriptManager.Actors.rifleProjectile);
                    gameObject.GetComponent<RifleProjectileBehaviour>()?.Shoot(m_origin, end);
                    Dispose();
                }
            }
        }

        private sealed class RocketProjectile : Projectile
        {
            public RocketProjectile(Vector3 _direction, Vector3 _origin, double _shootTime, byte _shooterId) : base(_direction, _origin, _shootTime, _shooterId)
            {
            }

            protected override void UpdateTrajectory(double _time, IValidationTarget _target)
            {
                Dispose();
            }
        }
    }
}