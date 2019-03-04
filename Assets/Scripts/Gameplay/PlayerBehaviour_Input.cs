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

        private const bool c_enablePartialSimulation = true;

        public bool isInteractive;

        private InputState m_accumulatedInput;
        private SimulationState m_lastSimulationState;

        private void AccumulateInput(InputState _inputState, float _deltaTime)
        {
            float timeFactor = _deltaTime / c_timestep;
            m_accumulatedInput.dash |= _inputState.dash;
            m_accumulatedInput.jump |= _inputState.jump;
            m_accumulatedInput.movementX += _inputState.movementX * timeFactor;
            m_accumulatedInput.movementZ += _inputState.movementZ * timeFactor;
        }

        public static void RotateMovementInputXZ(float _right, float _forward, float _turn, out float _out_x, out float _out_z)
        {
            float angleRad = Mathf.Deg2Rad * _turn;
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            _out_x = (cos * _right) + (sin * _forward);
            _out_z = (cos * _forward) - (sin * _right);
        }

        private float m_accumulatedTime = 0.0f;

        private void SendInput()
        {
            host.Moved(m_history.Newest, m_accumulatedInput, m_history[m_history.Newest].Value.simulation);
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


            // Rotate player
            Vector3 actAngles = characterController.transform.eulerAngles;
            actAngles.x = 0;
            //actAngles.y += inputLookX;
            actAngles.z = 0;
            gameObject.transform.eulerAngles = actAngles;

            // Rotate movement direction depending on turn angle
            RotateMovementInputXZ(inputMovX, inputMovY, actAngles.y, out float movementX, out float movementZ);


            // Current input state based on collected input
            InputState inputState = new InputState
            {
                jump = inputJump,
                movementX = movementX,
                movementZ = movementZ,
            };

            float timeToNextCommit = c_timestep - (m_accumulatedTime % c_timestep);
            m_accumulatedTime += Time.deltaTime;

            // Was there a full simulation during this update?
            bool unrolled = false;

            while (m_accumulatedTime >= c_timestep)
            {
                if (!unrolled)
                {
                    if (c_enablePartialSimulation)
                    {
                        // Undo all partial simulations
                        History.Node? node = m_history[m_history.Newest];
                        if (node != null)
                        {
                            ((History.Node) node).simulation.Apply(this);
                        }
                    }
                    // Finalize accumulated input
                    AccumulateInput(inputState, timeToNextCommit);
                }
                Simulate(m_accumulatedInput, c_timestep);
                m_history.Append(new History.Node { simulation = SimulationState.Capture(this), input = m_accumulatedInput });
                SendInput();
                // Jump and dash actions have already been taken into account
                inputState.jump = false;
                inputState.dash = false;
                m_accumulatedInput = inputState;
                m_accumulatedTime -= c_timestep;
                unrolled = true;
            }

            float timeSinceLastSimulation;

            if (unrolled)
            {
                ResetInput();
                timeSinceLastSimulation = m_accumulatedTime;
            }
            else
            {
                if (c_enablePartialSimulation)
                {
                    m_lastSimulationState.Apply(this);
                }
                timeSinceLastSimulation = Time.deltaTime;
            }

            AccumulateInput(inputState, timeSinceLastSimulation);
            if (c_enablePartialSimulation)
            {
                // Do a partial simulation
                Simulate(inputState, timeSinceLastSimulation);
                m_lastSimulationState = SimulationState.Capture(this);
            }

        }

    }
}
