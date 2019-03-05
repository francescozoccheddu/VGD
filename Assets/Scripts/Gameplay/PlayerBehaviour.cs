using UnityEngine;
using Wheeled.Core;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour : MonoBehaviour
    {

        internal IPlayerEventListener host;

        private void Update()
        {
            // Update actor time
            m_timeSinceLastPresentationNode += Time.deltaTime;
            m_presentationNode += Mathf.FloorToInt(m_timeSinceLastPresentationNode / c_timestep);
            m_timeSinceLastPresentationNode %= c_timestep;

            // Clamp to history tail
            Clamp();

            ProcessInput();
            ConfirmSimulation();
            UpdateActor();
        }

    }
}
