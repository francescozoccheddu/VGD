using UnityEngine;

namespace Wheeled.Gameplay.PlayerView
{

    public class DamperBehaviour : MonoBehaviour
    {

        public const float c_legsAngle = 15;
        public const float c_axisLength = 0.1f;
        public const float c_maxPistonHeight = 0.12f;
        public const float c_minPistonHeight = 0.01f;
        public const float c_minWheelDistance = 0.13f;
        public const float c_maxWheelDistance = 0.35f;
        public const float c_baseWheelDistance = 0.15f;

        [Header("Legs")]
        public Transform[] leftPistons;
        public Transform[] rightPistons;

        [Header("Wheel")]
        public Transform axis;
        public Transform wheel;

        [Header("Hands")]
        public Transform leftHand;
        public Transform rightHand;

        [Header("Properties")]
        public float height;
        public float smoothSpeed = 1f;
        private float m_height;

        private void Update()
        {
            const float maxHeight = c_maxWheelDistance - c_baseWheelDistance;
            m_height = Mathf.Min(height, Mathf.Lerp(m_height, Mathf.Min(height, maxHeight), Time.deltaTime * smoothSpeed));

            float wheelDistance = Mathf.Clamp(m_height + c_baseWheelDistance, c_minWheelDistance, c_maxWheelDistance);
            {
                // Wheel
                Vector3 position = wheel.localPosition;
                position.y = -wheelDistance;
                wheel.localPosition = position;
            }
            {
                // Pistons
                float length = wheelDistance / Mathf.Cos(c_legsAngle * Mathf.Deg2Rad);
                float remainingLenght = length - c_minPistonHeight * leftPistons.Length;
                for (int i = leftPistons.Length - 1; i >= 0; i--)
                {
                    float y = Mathf.Clamp(remainingLenght, c_minPistonHeight, c_maxPistonHeight);
                    remainingLenght -= y - c_minPistonHeight;
                    Vector3 position = leftPistons[i].localPosition;
                    position.y = -y;
                    leftPistons[i].localPosition = position;
                    rightPistons[i].localPosition = position;
                }
            }
            {
                // Hands
                float deltaX = Mathf.Sin(c_legsAngle * Mathf.Deg2Rad) * wheelDistance;
                float width = leftPistons[0].parent.localPosition.x;
                Vector3 position = rightHand.localPosition;
                position.x = -width - deltaX;
                rightHand.localPosition = position;
                position.x *= -1.0f;
                leftHand.localPosition = position;
            }
            {
                // Axis
                Vector3 scale = axis.localScale;
                float axisLength = rightHand.localPosition.x - leftHand.localPosition.x;
                scale.x = axisLength / c_axisLength;
                axis.localScale = scale;
            }
        }
    }

}