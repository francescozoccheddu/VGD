using UnityEngine;
using Wheeled.Core;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        internal IPlayerEventListener host;

        private Time m_presentationTime;

        private void Update()
        {
            m_presentationTime += UnityEngine.Time.deltaTime;

            Clamp();

            UpdateStatus();
            if (isInteractive)
            {
                ProcessInput();
            }
            else if (isAuthoritative)
            {
                ConfirmSimulation();
            }
            UpdateActor();
        }

    }
}
