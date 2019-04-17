using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Gameplay.Stage
{
    internal sealed class OffenseStage
    {

        public struct RifleShot
        {
            public Vector3 start;
            public Vector3 end;
            public bool hit;
            public float power;
        }

        public class RocketShotDistance
        {
            public RocketShotDistance(float _distance)
            {
                Distance = _distance;
            }
            public float Distance { get; }
        }

        public struct RocketShot
        {
            public Vector3 start;
            public Vector3 direction;
            public RocketShotDistance distance;
        }

        private struct PendingRocketShot
        {
            public RocketShot shot;
            public RocketProjectileBehaviour projectile;
        }

        public struct Explosion
        {
            public Vector3 position;
        }

        private readonly LinkedListHistory<double, RifleShot> m_rifleShots;
        private readonly LinkedListHistory<double, RocketShot> m_rocketShots;
        private readonly LinkedListHistory<double, Explosion> m_explosions;
        private readonly List<PendingRocketShot> m_pendingRocketShots;

        public OffenseStage()
        {
            m_rifleShots = new LinkedListHistory<double, RifleShot>();
            m_rocketShots = new LinkedListHistory<double, RocketShot>();
            m_explosions = new LinkedListHistory<double, Explosion>();
            m_pendingRocketShots = new List<PendingRocketShot>();
        }

        public void Put(double _time, in RifleShot _offense)
        {
            m_rifleShots.Add(_time, _offense);
        }

        public void Put(double _time, object _key, in RocketShot _offense)
        {
            m_rocketShots.Add(_time, _offense);
        }

        public void Put(double _time, in Explosion _offense)
        {
            m_explosions.Add(_time, _offense);
        }

        public void Update(double _time)
        {

        }

    }
}