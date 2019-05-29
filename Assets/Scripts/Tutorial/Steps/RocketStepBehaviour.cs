using UnityEngine;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Tutorial.Steps
{

    public sealed class RocketStepBehaviour : TutorialStepBehaviour
    {

        private void Awake()
        {
            m_Player.EnableRocket = true;
        }

        private void Update()
        {
            if (ActionController.IsShootingRocket)
            {
                Complete();
            }
        }
    }
}
