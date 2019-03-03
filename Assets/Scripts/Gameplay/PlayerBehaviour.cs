using UnityEngine;
using Wheeled.Core;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        internal IPlayerEventListener host;

        private void Update()
        {
            ProcessInput();
            UpdateActor();
        }

    }
}
