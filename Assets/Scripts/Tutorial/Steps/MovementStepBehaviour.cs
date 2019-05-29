using UnityEngine;

namespace Wheeled.Tutorial.Steps
{

    public sealed class MovementStepBehaviour : TutorialStepBehaviour
    {

        [Range(1.0f, 20.0f)]
        public float minDistance = 1.0f;

        private Vector3 m_origin;

        private void Awake()
        {
            m_origin = m_Player.Snapshot.simulation.Position;
            m_Player.EnableMovement = true;
        }

        private void Update()
        {
            if (Vector3.Distance(m_Player.Snapshot.simulation.Position, m_origin) >= minDistance)
            {
                Complete();
            }
        }
    }
}
