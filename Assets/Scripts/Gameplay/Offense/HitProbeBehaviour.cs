using System.Linq;
using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Stage
{
    public sealed class HitProbeBehaviour : MonoBehaviour
    {
        #region Internal Properties

        internal byte PlayerId { get; private set; }

        #endregion Internal Properties

        #region Public Fields

        public Collider[] criticalColliders;

        #endregion Public Fields

        #region Internal Methods

        internal void Disable()
        {
            gameObject?.SetActive(false);
        }

        internal bool IsCriticalCollider(Collider _collider)
        {
            return criticalColliders?.Contains(_collider) ?? false;
        }

        internal void Set(byte _playerId, Snapshot _snapshot)
        {
            PlayerId = _playerId;
            gameObject.SetActive(true);
            transform.position = _snapshot.simulation.Position;
            transform.rotation = _snapshot.sight.Quaternion;
        }

        #endregion Internal Methods
    }
}