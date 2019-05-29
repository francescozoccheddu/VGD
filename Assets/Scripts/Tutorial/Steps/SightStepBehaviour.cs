using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Tutorial.Steps
{

    public sealed class SightStepBehaviour : TutorialStepBehaviour
    {

        [Range(1.0f, 180.0f)]
        public float minAngle = 10.0f;

        private Sight m_origin;

        private void Awake()
        {
            m_origin = m_Player.Snapshot.sight;
            m_Player.Controller.EnableSight = true;
        }

        private void Update()
        {
            Snapshot snapshot = m_Player.Snapshot;
            if (Mathf.Abs(Mathf.DeltaAngle(snapshot.sight.Turn, m_origin.Turn)) >= minAngle
                || Mathf.Abs(Mathf.DeltaAngle(snapshot.sight.LookUp, m_origin.LookUp)) >= minAngle)
            {
                Complete();
            }
        }
    }
}
