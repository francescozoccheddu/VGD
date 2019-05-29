using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay.Player;

namespace Wheeled.Tutorial
{
    public abstract class TutorialStepBehaviour : MonoBehaviour
    {

        public DirectorBehaviour director;
        public int step;
        [Range(0.0f, 2.0f)]
        public float completionDelay;

        protected TutorialPlayer m_Player => (TutorialPlayer) GameManager.Current.LocalPlayer;

        protected void Complete()
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                director.Complete(step, completionDelay);
                enabled = false;
            }
        }

        public bool IsCompleted { get; private set; }

    }
}
