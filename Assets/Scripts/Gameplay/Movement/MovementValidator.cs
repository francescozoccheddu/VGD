#define OUTPUT_PARTIAL_SIMULATION

using UnityEngine;
using Wheeled.Core;

namespace Wheeled.Gameplay.Movement
{

    internal sealed class MovementValidator : IUpdatable
    {

        public interface IValidationTarget
        {

#if OUTPUT_PARTIAL_SIMULATION
            void Validated(int _step, InputStep _input, SimulationStep _simulation);
#else
            void Validated(int _step, SimulationStep _simulation);
#endif

        }

        public interface ICorrectionTarget
        {

            void Corrected(int _step, SimulationStep _simulation);
            void Rejected(int _step, bool _newer);

        }

        public int Step { get; private set; }
        public bool IsRunning { get; private set; }
        public int maxTrustedSteps;

        public IValidationTarget validationTarget;
        public ICorrectionTarget correctionTarget;

        private struct Node
        {
            public InputStep? input;
            public SimulationStep? simulation;
        }

        private readonly Node[] m_buffer;
        private int m_Length => m_buffer.Length;
        private SimulationStep m_simulation;
        private InputStep m_input;
        private int m_trustedSteps;

        private int GetStep(int _step)
        {
            return _step % m_Length;
        }

        public MovementValidator(float _duration)
        {
            m_buffer = new Node[Mathf.CeilToInt(_duration / TimeStep.c_simulationStep)];
        }

        public void SendCorrection()
        {
            m_trustedSteps = 0;
            correctionTarget?.Corrected(Step, m_simulation);
        }

        public void ClearBuffer()
        {
            for (int i = 0; i < m_buffer.Length; i++)
            {
                m_buffer[i] = new Node();
            }
        }

        public void StartAt(int _step, bool _clearBuffer)
        {
            if (_clearBuffer)
            {
                ClearBuffer();
            }
            else
            {
                int oldL = Step;
                int oldR = Step + m_Length - 1;
                int newL = _step;
                int newR = _step + m_Length - 1;
                if (newL > oldL && newL <= oldR)
                {
                    Node[] clone = new Node[oldR - newL + 1];
                    for (int i = 0; i < clone.Length; i++)
                    {
                        clone[i] = m_buffer[GetStep(i + newL)];
                    }
                    ClearBuffer();
                    for (int i = 0; i < clone.Length; i++)
                    {
                        m_buffer[GetStep(i + newL)] = clone[i];
                    }
                }
                else if (newR >= oldL && newR < oldR)
                {
                    Node[] clone = new Node[newR - oldL + 1];
                    for (int i = 0; i < clone.Length; i++)
                    {
                        clone[i] = m_buffer[GetStep(i + newR)];
                    }
                    ClearBuffer();
                    for (int i = 0; i < clone.Length; i++)
                    {
                        m_buffer[GetStep(i + newR)] = clone[i];
                    }
                }
                else if (newL != oldL)
                {
                    ClearBuffer();
                }
            }
            IsRunning = true;
            Step = _step;
        }

        public void Teleport(in SimulationStep _simulation)
        {
            m_simulation = _simulation;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Put(int _firstStep, InputStep[] _inputSteps, in SimulationStep _simulation)
        {
            for (int i = 0; i < _inputSteps.Length; i++)
            {
                int step = i + _firstStep;
                if (step < Step)
                {
                    correctionTarget?.Rejected(step, false);
                }
                else if (step >= Step + m_Length)
                {
                    correctionTarget?.Rejected(step, true);
                }
                else
                {
                    m_buffer[GetStep(step)].input = _inputSteps[i];
                }
            }
            if (_firstStep + _inputSteps.Length < Step + m_Length)
            {
                m_buffer[GetStep(_firstStep + m_Length - 1)].simulation = _simulation;
            }
        }

        private void Validate()
        {
            int bufInd = GetStep(Step);
            InputStep input = m_buffer[bufInd].input ?? m_input.Predicted;
            m_simulation = m_simulation.Simulate(input, TimeStep.c_simulationStep);
#if OUTPUT_PARTIAL_SIMULATION
            validationTarget?.Validated(Step, input, m_simulation);
#else
            validationTarget?.Validated(Step, m_simulation);
#endif
            if (m_buffer[bufInd].simulation != null)
            {
                if (!SimulationStep.IsNearlyEqual(m_simulation, m_buffer[bufInd].simulation.Value))
                {
                    SendCorrection();
                }
            }
            else
            {
                m_trustedSteps++;
                if (m_trustedSteps > maxTrustedSteps)
                {
                    SendCorrection();
                }
            }
            m_buffer[bufInd] = new Node();
            Step++;
        }

        public void Update()
        {
            if (IsRunning)
            {
                while (m_buffer[GetStep(Step)].input != null)
                {
                    Validate();
                }
                while (Step < RoomTime.Now.Step)
                {
                    Validate();
                }
            }
        }

    }

}
