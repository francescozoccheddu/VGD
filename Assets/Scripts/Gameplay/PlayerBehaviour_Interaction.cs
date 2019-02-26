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
            public float turn;
            public float lookUp;
        }

        public bool isInteractive;

        private InputState m_accumulatedInput;

        private const float c_maxDeltaTime = 1 / 10.0f;

        private void AccumulateInput(InputState _inputState, float _deltaTime)
        {
            m_accumulatedInput.dash |= _inputState.dash;
            m_accumulatedInput.jump |= _inputState.jump;
            m_accumulatedInput.lookUp = _inputState.lookUp;
            m_accumulatedInput.turn = _inputState.turn;
            m_accumulatedInput.movementX += _inputState.movementX * _deltaTime;
            m_accumulatedInput.movementZ += _inputState.movementZ * _deltaTime;
        }

        private void CommitInput()
        {

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

            float deltaTime = Mathf.Min(c_maxDeltaTime, Time.deltaTime);
            bool inputJump = Input.GetButtonDown("Jump");
            float inputMovX = Input.GetAxis("Horizontal");
            float inputMovY = Input.GetAxis("Vertical");
            float inputLookX = Input.GetAxis("Mouse X");
            float inputLookY = Input.GetAxis("Mouse Y");

            Vector3 actAngles = transform.eulerAngles;
            actAngles.x = 0;
            actAngles.y += inputLookX;
            actAngles.z = 0;

            gameObject.transform.eulerAngles = actAngles;

            InputState inputState = new InputState
            {
                jump = inputJump,
                movementX = inputMovX,
                movementZ = inputMovY,
                turn = actAngles.y,
                lookUp = actAngles.x
            };

            if (m_timeSinceLastUpdate + deltaTime >= c_timestep)
            {
                float timeToNextUpdate = c_timestep - m_timeSinceLastUpdate;
                Simulate(inputState, timeToNextUpdate);
                AccumulateInput(inputState, timeToNextUpdate);
                CommitInput();
                ResetInput();
                inputState.jump = false;
                inputState.dash = false;
                AccumulateInput(inputState, deltaTime - timeToNextUpdate);
                Simulate(inputState, deltaTime - timeToNextUpdate);
            }
            else
            {
                AccumulateInput(inputState, deltaTime);
                Simulate(inputState, deltaTime);
            }
        }

    }
}
