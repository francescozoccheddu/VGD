using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class CollisionScript : ScriptableObject
    {
        #region Public Fields

        public LayerMask shoot;
        public LayerMask movement;
        public LayerMask jumpPad;
        public LayerMask deathCameraVolume;

        #endregion Public Fields
    }
}