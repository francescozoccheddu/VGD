using System.Collections.Generic;

using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    internal sealed class MovementValidator
    {
        private readonly Node[] m_buffer;
        private SimulationStepInfo m_last;
        private int m_maxTrustedSteps;
        private int m_trustedSteps;
        // DEBUG
        private int m_noData = 0;
        private int m_wrongData = 0;

        public MovementValidator(double _duration)
        {
            m_buffer = new Node[_duration.CeilingSimulationSteps()];
            MaxTrustedSteps = 2;
        }

        public interface ITarget
        {
            void Corrected(int _step, in SimulationStepInfo _simulation);

            void Rejected(int _step, bool _newer);

            void Validated(int _step, in InputStep _input, in CharacterController _simulation);
        }

        public bool IsRunning { get; set; }

        public int MaxTrustedSteps
        {
            get => m_maxTrustedSteps;
            set
            {
                Debug.Assert(value > 0);
                m_maxTrustedSteps = value;
                if (m_trustedSteps > m_maxTrustedSteps)
                {
                    SendCorrection();
                }
            }
        }

        public int Step { get; private set; }
        public ITarget Target { get; set; }
        private int m_Length => m_buffer.Length;

        public void ClearBuffer()
        {
            for (int i = 0; i < m_buffer.Length; i++)
            {
                m_buffer[i] = new Node();
            }
        }

        public void Put(int _step, IEnumerable<InputStep> _reversedInputSteps, in CharacterController _simulation)
        {
            int step = _step;
            foreach (InputStep inputStep in _reversedInputSteps)
            {
                if (step < Step)
                {
                    Target?.Rejected(step, false);
                    break;
                }
                else if (step >= Step + m_Length)
                {
                    Target?.Rejected(step, true);
                }
                else
                {
                    m_buffer[GetStep(step)].input = inputStep;
                }
                step--;
            }
            if (_step < Step + m_Length)
            {
                m_buffer[GetStep(_step)].simulation = _simulation;
            }
        }

        public void SendCorrection()
        {
            m_trustedSteps = 0;
            Target?.Corrected(Step, m_last);
        }

        public void SkipTo(int _step, bool _clearBuffer)
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
            Step = _step;
        }

        public void Teleport(in CharacterController _simulation)
        {
            m_last.simulation = _simulation;
        }

        public void UpdateUntil(int _step)
        {
            if (IsRunning)
            {
                while (m_buffer[GetStep(Step)].input != null)
                {
                    Validate();
                }
                while (Step < _step)
                {
                    Validate();
                }
            }
        }

        private int GetStep(int _step)
        {
            return _step % m_Length;
        }

        private void Validate()
        {
            int bufInd = GetStep(Step);
            m_last.input = m_buffer[bufInd].input ?? m_last.input.Predicted;
            m_last.simulation = m_last.simulation.Simulate(m_last.input, (float) TimeConstants.c_simulationStep);
            Target?.Validated(Step, m_last.input, m_last.simulation);
            if (m_buffer[bufInd].simulation != null)
            {
                m_trustedSteps = 0;
                if (!CharacterController.AreNearlyEqual(m_last.simulation, m_buffer[bufInd].simulation.Value))
                {
                    SendCorrection();
                }
            }
            else
            {
                m_trustedSteps++;
                if (m_trustedSteps > m_maxTrustedSteps)
                {
                    SendCorrection();
                }
            }
            m_buffer[bufInd] = new Node();
            Step++;
        }

        private struct Node
        {
            public InputStep? input;
            public CharacterController? simulation;
        }
    }
}