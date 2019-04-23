using UnityEngine;

namespace Wheeled.Gameplay.Scene
{
    public sealed class DeathCameraBehaviour : MonoBehaviour
    {
        #region Public Fields

        public new Camera camera;
        public AudioListener audioListener;

        #endregion Public Fields

        #region Public Methods

        public void SetEnabled(bool _enabled)
        {
            camera.enabled = _enabled;
            audioListener.enabled = _enabled;
        }

        #endregion Public Methods
    }
}