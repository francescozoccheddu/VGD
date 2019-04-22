using UnityEngine;
using Wheeled.Core.Data;

namespace Wheeled.Gameplay.Scene
{
    internal static class DeathCamera
    {
        #region Private Fields

        private const float c_radius = 0.5f;
        private static GameObject s_lastCamera;

        #endregion Private Fields

        #region Public Methods

        public static void Enable(Vector3 _position)
        {
            Disable();
            Collider[] colliders = Physics.OverlapSphere(_position, c_radius, ScriptManager.Collisions.deathCameraVolume);
            foreach (Collider collider in colliders)
            {
                GameObject gameObject = collider.gameObject;
                if (gameObject != null)
                {
                    s_lastCamera = gameObject;
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
            s_lastCamera = GameObject.FindWithTag("DefaultDeathCamera");
            SetEnabled(true);
        }

        #endregion Public Methods

        #region Private Methods

        private static void SetEnabled(bool _enable)
        {
            if (s_lastCamera != null)
            {
                s_lastCamera.GetComponent<Camera>().enabled = true;
                s_lastCamera.GetComponent<AudioListener>().enabled = true;
            }
        }

        #endregion Private Methods
    }
}