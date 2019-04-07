using System.Linq;
using UnityEngine;
using Wheeled.Gameplay.Movement;
using Wheeled.Networking;

namespace Wheeled.Gameplay.Stage
{
    public sealed class HitProbeBehaviour : MonoBehaviour
    {
        public Collider[] criticalColliders;

        internal PlayerBase Player { get; private set; }

        internal void Disable()
        {
            gameObject?.SetActive(false);
        }

        internal bool IsCriticalCollider(Collider _collider)
        {
            return criticalColliders?.Contains(_collider) ?? false;
        }

        internal void Set(PlayerBase _player, Snapshot _snapshot)
        {
            Player = _player;
            gameObject.SetActive(true);
            transform.position = _snapshot.simulation.position;
            transform.rotation = _snapshot.sight.Quaternion;
        }
    }
}