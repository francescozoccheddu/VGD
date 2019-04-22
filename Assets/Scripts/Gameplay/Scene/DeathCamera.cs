using UnityEngine;
using Wheeled.Core.Data;

namespace Wheeled.Gameplay.Scene
{
    internal static class DeathCamera
    {
        #region Private Fields

        private const float c_radius = 0.5f;
        private static Camera s_lastCamera;

        #endregion Private Fields

        #region Public Methods

        public static void Enable(Vector3 _position)
        {
            Disable();
            Collider[] colliders = Physics.OverlapSphere(_position, c_radius, ScriptManager.Collisions.deathCameraVolume);
            foreach (Collider collider in colliders)
            {
                GameObject gameObject = collider.gameObject;
                Camera camera = gameObject.GetComponent<Camera>();
                if (camera != null)
                {
                    s_lastCamera = camera;
                    break;
                }
            }
            if (s_lastCamera == null)
            {
                EnableDefault();
            }
            else
            {
                s_lastCamera.enabled = true;
            }
        }

        public static void Disable()
        {
            if (s_lastCamera != null)
            {
                s_lastCamera.enabled = false;
            }
            s_lastCamera = null;
        }

        public static void EnableDefault()
        {
            Disable();
            s_lastCamera = GameObject.FindWithTag("DefaultDeathCamera")?.GetComponent<Camera>();
            if (s_lastCamera != null)
            {
                s_lastCamera.enabled = true;
            }
        }

        #endregion Public Methods
    }
}