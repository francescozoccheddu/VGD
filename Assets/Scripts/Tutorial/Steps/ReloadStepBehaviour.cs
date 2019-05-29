using UnityEngine;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Tutorial.Steps
{

    public sealed class ReloadStepBehaviour : TutorialStepBehaviour
    {

        private void Awake()
        {
            m_Player.Controller.EnableKaze = true;
        }

        private void Update()
        {
            if (ActionController.IsKazing)
            {
                Complete();
            }
        }
    }
}
