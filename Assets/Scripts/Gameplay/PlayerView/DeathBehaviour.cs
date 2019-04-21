using UnityEngine;

namespace Wheeled.Gameplay.PlayerView
{
    public sealed class DeathBehaviour : MonoBehaviour
    {
        #region Public Properties

        public bool IsDead { get; private set; }

        #endregion Public Properties

        #region Internal Methods

        internal void Die(Vector3 _velocity)
        {
            if (!IsDead)
            {
                IsDead = true;
                // Test only
                gameObject.SetActive(false);
                GetComponent<CameraBehaviour>().camera.enabled = false;
            }
        }

        #endregion Internal Methods
    }
}