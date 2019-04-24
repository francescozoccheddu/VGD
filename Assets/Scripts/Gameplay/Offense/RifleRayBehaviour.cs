using UnityEngine;

namespace Wheeled.Gameplay.Stage
{
    public sealed class RifleRayBehaviour : MonoBehaviour
    {
        #region Public Methods

        public void Destroy()
        {
            Destroy(gameObject);
        }

        #endregion Public Methods
    }
}