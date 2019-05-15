﻿using System.Linq;
using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Offense
{
    public sealed class HitProbeBehaviour : MonoBehaviour
    {
        public byte PlayerId { get; private set; }

        public Collider[] criticalColliders;

        public void Disable()
        {
            gameObject?.SetActive(false);
        }

        public bool IsCriticalCollider(Collider _collider)
        {
            return criticalColliders?.Contains(_collider) ?? false;
        }

        public void Set(byte _playerId, Snapshot _snapshot)
        {
            PlayerId = _playerId;
            gameObject.SetActive(true);
            transform.position = _snapshot.simulation.Position;
            transform.rotation = _snapshot.sight.Quaternion;
        }
    }
}