using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        private void Update()
        {
            ProcessInput();
        }

        public void Destroy()
        {
            if (gameObject == null)
            {
                return;
            }
            Destroy(gameObject);
        }

    }
}
