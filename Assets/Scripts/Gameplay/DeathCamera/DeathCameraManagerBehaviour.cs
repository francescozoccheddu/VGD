using UnityEngine;

namespace Wheeled.Gameplay.DeathCamera
{
    public sealed class DeathCameraManagerBehaviour : MonoBehaviour
    {
        #region Internal Interfaces

        internal interface ITarget
        {
            #region Public Methods

            Vector3 GetPosition();

            #endregion Public Methods
        }

        #endregion Internal Interfaces

        #region Internal Properties

        internal static DeathCameraManagerBehaviour Instance { get; private set; }

        #endregion Internal Properties

        #region Public Fields

        public Camera defaultCamera;
        public DeathCameraBehaviour[] cameras;

        #endregion Public Fields

        #region Private Fields

        private ITarget m_target;
        private DeathCameraBehaviour m_current;

        #endregion Private Fields

        #region Internal Methods

        internal void Enable(ITarget _target)
        {
            m_target = _target;
        }

        internal void Disable()
        {
        }

        #endregion Internal Methods

        #region Private Methods

        private void Start()
        {
            Instance = this;
        }

        #endregion Private Methods
    }
}