using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour
    {

        public CharacterController characterController;

        public const float speed = 5.0f;
        public const float jumpForce = 10.0f;

        public const float movementForce = 100.0f;
        public const float dashImpulse = 1.0f;
        public const float jumpImpulse = 5.0f;
        public const float groundDragForce = 5.0f;
        public const float airDragForce = 1.0f;
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

        public struct SimulationState
        {

            public Vector3 position;
            public float lookUp;
            public float turn;
            public Vector3 velocity;
            public float dashStamina;

            public void Apply(PlayerBehaviour playerController)
            {
                playerController.transform.position = position;
                playerController.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                playerController.m_velocity = velocity;
                playerController.m_dashStamina = dashStamina;
            }

            public static SimulationState Capture(PlayerBehaviour playerController)
            {
                return new SimulationState
                {
                    position = playerController.transform.position,
                    lookUp = playerController.transform.eulerAngles.x,
                    turn = playerController.transform.eulerAngles.y,
                    velocity = playerController.m_velocity,
                    dashStamina = playerController.m_dashStamina
                };
            }

            public static SimulationState Lerp(SimulationState a, SimulationState b, float progress)
            {
                return new SimulationState
                {
                    position = Vector3.Lerp(a.position, b.position, progress),
                    lookUp = Mathf.Lerp(a.lookUp, b.lookUp, progress),
                    turn = Mathf.Lerp(a.turn, b.turn, progress),
                    velocity = Vector3.Lerp(a.velocity, b.velocity, progress),
                    dashStamina = Mathf.Lerp(a.dashStamina, b.dashStamina, progress)
                };
            }

        }

        private void Simulate(InputState input, float deltaTime)
        {
            // Simulate gravity first, in order to update characterController.isGrounded
            characterController.Move(new Vector3(0.0f, m_velocity.y, 0.0f) * deltaTime);
            // Simulate XZ movement
            float dragForce = airDragForce;
            if (characterController.isGrounded)
            {
                m_velocity.x += input.movementX;
                m_velocity.y = input.jump ? jumpImpulse : 0.0f;
                m_velocity.z += input.movementZ;
                dragForce += groundDragForce;
            }
            m_velocity.x = UpdateSpeed(m_velocity.x, dragForce, maxSpeed, deltaTime);
            m_velocity.y -= gravityForce * deltaTime;
            m_velocity.z = UpdateSpeed(m_velocity.z, dragForce, maxSpeed, deltaTime);
            characterController.Move(new Vector3(m_velocity.x, 0.0f, m_velocity.z) * deltaTime);
        }

        private const float c_timestep = 1 / 30.0f;

        private SimulationState m_simulationState;

    }

}