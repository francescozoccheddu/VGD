using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Tutorial.Steps
{

    public sealed class JumpStepBehaviour : TutorialStepBehaviour
    {

        private void Awake()
        {
            m_Player.EnableJump = true;
        }

        private void Update()
        {
            if (MovementController.IsJumping)
            {
                Complete();
            }
        }
    }
}
