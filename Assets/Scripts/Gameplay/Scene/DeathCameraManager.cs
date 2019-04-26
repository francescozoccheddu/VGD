using UnityEngine;
using Wheeled.Core.Data;

namespace Wheeled.Gameplay.Scene
{
    internal static class DeathCameraManager
    {
        #region Private Fields

        private const float c_radius = 0.5f;
        private static DeathCameraBehaviour s_lastCamera;

        #endregion Private Fields

        #region Public Methods

        public static void Enable(Vector3 _position)
        {
            Disable();
            Collider[] colliders = Physics.OverlapSphere(_position, c_radius, Scripts.Collisions.deathCameraVolume);
            foreach (Collider collider in colliders)
            {
                s_lastCamera = collider.gameObject.GetComponent<DeathCameraBehaviour>();
                if (s_lastCamera != null)
                {
                    break;
                }
            }
            if (s_lastCamera == null)
            {
                EnableDefault();
            }
            else
            {
                SetEnabled(true);
            }
        }

        public static void Disable()
        {
            SetEnabled(false);
            s_lastCamera = null;
        }

        public static void EnableDefault()
        {
            Disable();
            s_lastCamera = GameObject.FindWithTag("DefaultDeathCamera").GetComponent<DeathCameraBehaviour>();
            SetEnabled(true);
        }

        #endregion Public Methods

        #region Private Methods

        private static void SetEnabled(bool _enable)
        {
            if (s_lastCamera != null)
            {
                s_lastCamera.SetEnabled(_enable);
            }
        }

        #endregion Private Methods
    }
}