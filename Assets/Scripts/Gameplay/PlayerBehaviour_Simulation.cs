using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour
    {

        public CharacterController characterController;
        public float speed;
        public float jumpForce;

        public const float movementForce = 100.0f;
        public const float dashImpulse = 1.0f;
        public const float jumpImpulse = 5.0f;
        public const float groundDragForce = 50.0f;
        public const float airDragForce = 5.0f;
        public const float dashStaminaGrowth = 1.0f;
        public const bool relativeDashImpulse = false;
        public const float gravityForce = 10.0f;
        public const float maxSpeed = 10.0f;

        private float m_dashStamina;
        private Vector3 m_velocity;

        private static float UpdateSpeed(float speed, float drag, float max, float deltaTime)
        {
            if (speed > 0)
            {
                speed = speed - drag * deltaTime;
                return speed < 0.0f ? 0.0f : speed > max ? max : speed;
            }
            else
            {
                speed = speed + drag * deltaTime;
                return speed > 0.0f ? 0.0f : speed < -max ? -max : speed;
            }
        }

        public static Vector2 RotateMovementInputXZ(float right, float forward, float turn)
        {
            float angleRad = Mathf.Deg2Rad * turn;
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            return new Vector2
            {

                x = (cos * right) + (sin * forward),
                y = (cos * forward) - (sin * right)
            };
        }

        private void Simulate(InputState input, float deltaTime)
        {
            float dragForce = airDragForce;
            if (characterController.isGrounded)
            {
                if (input.jump)
                {
                    m_velocity.y = jumpImpulse;
                }
                m_velocity.x += input.movementX;
                m_velocity.z += input.movementZ;
                dragForce += groundDragForce;
            }
            m_velocity.x = UpdateSpeed(m_velocity.x, dragForce, maxSpeed, deltaTime);
            m_velocity.y -= gravityForce * deltaTime;
            m_velocity.z = UpdateSpeed(m_velocity.z, dragForce, maxSpeed, deltaTime);
            characterController.Move(m_velocity * deltaTime);
        }

        private const float c_timestep = 1 / 40.0f;

        private float m_timeSinceLastUpdate = 0.0f;

    }

}