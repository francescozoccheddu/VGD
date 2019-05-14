using UnityEngine;

namespace Wheeled.Gameplay.Scene
{
    public sealed class DeathCameraBehaviour : MonoBehaviour
    {

        public new GameObject camera;

        public void SetEnabled(bool _enabled)
        {
            camera.SetActive(_enabled);
        }

    }
}