using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Tutorial.Steps
{

    public sealed class DashStepBehaviour : TutorialStepBehaviour
    {

        private void Awake()
        {
            m_Player.Controller.EnableDash = true;
        }

        private void Update()
        {
            if (MovementController.IsDashing && new Vector2(MovementController.HorizontalMovement, MovementController.VerticalMovement).magnitude > 0.1f)
            {
                Complete();
            }
        }
    }
}
