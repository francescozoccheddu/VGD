using UnityEngine;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Tutorial.Steps
{

    public sealed class RifleStepBehaviour : TutorialStepBehaviour
    {

        private void Awake()
        {
            m_Player.Controller.EnableRifle = true;
        }

        private void Update()
        {
            if (ActionController.IsShootingRifle)
            {
                Complete();
            }
        }
    }
}
