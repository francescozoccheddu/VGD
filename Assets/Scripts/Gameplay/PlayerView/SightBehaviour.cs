using UnityEngine;
using Wheeled.Sound;

namespace Wheeled.Gameplay.PlayerView
{

    public class SightBehaviour : MonoBehaviour
    {

        [Header("Transforms")]
        public Transform head;
        public Transform arm;
        public Transform torso;
        public Transform cameraArm;

        [Header("LookUp")]
        public float lookUpTime = 5.0f;
        public float maxLookUpOffset = 30.0f;
        public float headLookUpFactor = 0.3f;
        private float m_lookUp;

        [Header("Turn")]
        public float turnTime = 5.0f;
        public float maxTurnOffset = 30.0f;
        private float m_turn;

        [Header("Sight")]
        public float lookUp;
        public float turn;

        [Header("Sounds")]
        public ContinuousAudioPlayerBehaviour turnSound;
        public ContinuousAudioPlayerBehaviour lookUpSound;

        public void ReachTarget()
        {
            m_turn = turn;
            m_lookUp = lookUp;
            m_turnVelocity = 0.0f;
            m_lookUpVelocity = 0.0f;
        }

        private float m_turnVelocity;
        private float m_lookUpVelocity;

        private float Lerp(float _current, float _target, float _smoothTime, ref float _refVelocity, float _maxOffset)
        {
            float angle = Mathf.SmoothDampAngle(_current, _target, ref _refVelocity, _smoothTime);
            float diff = Mathf.DeltaAngle(angle, _target);
            if (Mathf.Abs(diff) > _maxOffset)
            {
                angle += diff - Mathf.Sign(diff) * _maxOffset;
            }
            return angle;
        }

        private void Update()
        {
            m_turn = Lerp(m_turn, turn, turnTime, ref m_turnVelocity, maxTurnOffset);
            m_lookUp = Lerp(m_lookUp, lookUp, lookUpTime, ref m_lookUpVelocity, maxLookUpOffset);

            head.localRotation = Quaternion.Euler(m_lookUp * headLookUpFactor, 0.0f, 0.0f);
            arm.localRotation = Quaternion.Euler(m_lookUp, 0.0f, 0.0f);
            torso.localRotation = Quaternion.Euler(0.0f, m_turn, 0.0f);
            cameraArm.localRotation = Quaternion.Euler(lookUp, turn, 0.0f);

            lookUpSound.value = lookUp;
            turnSound.value = turn;
        }
    }
}