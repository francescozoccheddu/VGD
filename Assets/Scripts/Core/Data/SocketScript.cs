using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class SocketScript : ScriptableObject
    {
        #region Public Fields

        public Vector3 rifleBarrel;
        public Vector3 rocketBarrel;
        public Vector3 raycastOrigin;

        #endregion Public Fields
    }
}