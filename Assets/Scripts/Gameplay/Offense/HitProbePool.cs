using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Offense
{
    public sealed class HitProbePool
    {
        public struct HitInfo
        {
            public bool isCritical;
            public Vector3 normal;
            public int? playerId;
            public Vector3 position;
        }

        private const int c_worldLayerMask = 1 << 11 | 1 << 12;
        private readonly List<HitProbeBehaviour> m_probes;
        private int m_nextProbe;

        public HitProbePool()
        {
            m_probes = new List<HitProbeBehaviour>();
        }

        public void Add(int _playerId, Snapshot _snapshot)
        {
            while (m_nextProbe >= m_probes.Count)
            {
                m_probes.Add(null);
            }
            if (m_probes[m_nextProbe] == null)
            {
                m_probes[m_nextProbe] = Object.Instantiate(Scripts.Actors.collisionProbe).GetComponent<HitProbeBehaviour>();
            }
            m_probes[m_nextProbe].Set(_playerId, _snapshot);
            m_nextProbe++;
        }

        public void Clear()
        {
            foreach (HitProbeBehaviour p in m_probes)
            {
                p?.Disable();
            }
            m_nextProbe = 0;
        }

        public void Dispose()
        {
            foreach (HitProbeBehaviour p in m_probes)
            {
                if (p?.gameObject != null)
                {
                    Object.Destroy(p.gameObject);
                }
            }
            m_probes.Clear();
            m_nextProbe = 0;
        }

        public bool RayCast(Vector3 _start, Vector3 _end, out HitInfo _outInfo)
        {
            Vector3 diff = _end - _start;
            Ray ray = new Ray(_start, diff);
            int mask = Scripts.Collisions.shoot | (1 << Scripts.Actors.collisionProbe.layer);
            if (Physics.Linecast(_start, _end, out RaycastHit hit, mask))
            {
                GameObject gameObject = hit.collider.gameObject;
                HitProbeBehaviour hitProbe = gameObject.GetComponent<HitProbeBehaviour>();
                _outInfo = new HitInfo
                {
                    position = hit.point,
                    normal = hit.normal,
                    playerId = hitProbe?.PlayerId,
                    isCritical = hitProbe?.IsCriticalCollider(hit.collider) ?? false
                };
                return true;
            }
            else
            {
                _outInfo = default;
                return false;
            }
        }
    }
}