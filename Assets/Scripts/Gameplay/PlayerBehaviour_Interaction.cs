using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour
    {

        public struct InputState
        {
            public float movementZ;
            public float movementX;
            public bool jump;
            public bool dash;
        }

        public bool isInteractive;

        private InputState m_accumulatedInput;

        private const float c_maxDeltaTime = 1 / 10.0f;

        private void AccumulateInput(InputState _inputState, float _deltaTime)
        {
            float timeFactor = _deltaTime / c_timestep;
            m_accumulatedInput.dash |= _inputState.dash;
            m_accumulatedInput.jump |= _inputState.jump;
            m_accumulatedInput.movementX += _inputState.movementX * timeFactor;
            m_accumulatedInput.movementZ += _inputState.movementZ * timeFactor;
        }

        public static void RotateMovementInputXZ(float _right, float _forward, float _turn, out float o_x, out float o_z)
        {
            float angleRad = Mathf.Deg2Rad * _turn;
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            o_x = (cos * _right) + (sin * _forward);
            o_z = (cos * _forward) - (sin * _right);
        }

        private float m_timeSinceLastCommit = 0.0f;

        private void CommitInput()
        {
            m_timeSinceLastCommit = 0.0f;
        }

        private void ResetInput()
        {
            m_accumulatedInput.dash = false;
            m_accumulatedInput.jump = false;
            m_accumulatedInput.movementX = 0.0f;
            m_accumulatedInput.movementZ = 0.0f;
        }

        private void ProcessInput()
        {
            if (!isInteractive)
            {
                return;
            }

            bool inputJump = Input.GetButtonDown("Jump");
            float inputMovX = Input.GetAxis("Horizontal");
            float inputMovY = Input.GetAxis("Vertical");
            float inputLookX = Input.GetAxis("Mouse X");
            float inputLookY = Input.GetAxis("Mouse Y");

            Vector3 actAngles = transform.eulerAngles;
            actAngles.x = 0;
            actAngles.y += inputLookX;
            actAngles.z = 0;

            RotateMovementInputXZ(inputMovX, inputMovY, actAngles.y, out float movementX, out float movementZ);

            gameObject.transform.eulerAngles = actAngles;

            InputState inputState = new InputState
            {
                jump = inputJump,
                movementX = movementX,
                movementZ = movementZ,
            };

            float timeToNextCommit = c_timestep - (m_timeSinceLastCommit % c_timestep);
            m_timeSinceLastCommit += Time.deltaTime;

            bool unrolled = false;

            while (m_timeSinceLastCommit >= c_timestep)
            {
                if (!unrolled)
                {
                    // jump to last simulation state
                    AccumulateInput(inputState, timeToNextCommit);
                    CommitInput();
                    Simulate(m_accumulatedInput, c_timestep);
                    inputState.jump = false;
                    inputState.dash = false;
                    m_accumulatedInput = inputState;
                }
                else
                {
                    CommitInput();
                    Simulate(m_accumulatedInput, c_timestep);
                }
                m_timeSinceLastCommit -= c_timestep;
                unrolled = true;
            }

            float timeSinceLastSimulation;

            if (unrolled)
            {
                ResetInput();
                timeSinceLastSimulation = m_timeSinceLastCommit;
            }
            else
            {
                timeSinceLastSimulation = Time.deltaTime;
            }

            AccumulateInput(inputState, timeSinceLastSimulation);
            //Simulate(inputState, timeSinceLastSimulation);
        }

    }
}
