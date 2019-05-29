using UnityEngine;
using Wheeled.Gameplay.Movement;
using Wheeled.UI.HUD;

namespace Wheeled.Tutorial.Steps
{

    public sealed class ScoreBoardStepBehaviour : TutorialStepBehaviour
    {

        public ScoreBoardBehaviour scoreBoard;

        private void Awake()
        {
            scoreBoard.enabled = true;
        }

        private void Update()
        {
            if (scoreBoard.IsOpen)
            {
                Complete();
            }
        }
    }
}
