using UnityEngine;

namespace Wheeled.Gameplay.PlayerView
{
    public class WheelBehaviour : MonoBehaviour
    {

        public const float c_radius = 0.22f;
        private const float c_sampleSmoothQuickness = 8f;
        private const float c_decelerationSmoothQuickness = 2f;

        [Header("Transforms")]
        public Transform wheel;
        public Transform hip;

        [Header("Properties")]
        public float smoothSpeed = 10.0f;

        private Vector3 m_lastPosition;
        private float m_hipTurn;

        private float m_angularSpeed;

        public bool isGrounded;

        private void Update()
        {
            Vector3 offset = transform.position - m_lastPosition;
            Vector2 offsetXZ = new Vector2(offset.x, offset.z);
            float angularSpeed;
            if (isGrounded)
            {
                if (offsetXZ != Vector2.zero)
                {
                    float targetAngle = Vector2.SignedAngle(offsetXZ, Vector2.up);
                    float currentAngle = hip.localEulerAngles.y;
                    float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
                    {
                        float altDelta = Mathf.DeltaAngle(currentAngle + 180.0f, targetAngle);
                        float path = Mathf.Abs(delta) < Mathf.Abs(altDelta) ? delta : altDelta;
                        m_hipTurn = currentAngle + path;
                    }
                    {
                        float cos = Mathf.Cos(delta * Mathf.Deg2Rad);
                        float distance = offset.magnitude * cos;
                        const float circumference = c_radius * 2.0f * Mathf.PI;
                        angularSpeed = distance / circumference * 360.0f;
                    }
                }
                else
                {
                    angularSpeed = 0.0f;
                }
                m_angularSpeed = Mathf.Lerp(m_angularSpeed, angularSpeed, Time.deltaTime * c_sampleSmoothQuickness);
            }
            else
            {
                m_angularSpeed = angularSpeed = Mathf.Lerp(m_angularSpeed, 0, Time.deltaTime * c_decelerationSmoothQuickness);
            }
            wheel.Rotate(Vector3.right, angularSpeed);
            float turn = Mathf.LerpAngle(hip.localEulerAngles.y, m_hipTurn, Time.deltaTime * smoothSpeed);
            hip.localEulerAngles = new Vector3(0, turn, 0);
            m_lastPosition = transform.position;
        }
    }
}