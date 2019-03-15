﻿
using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Gameplay.Movement
{

    internal sealed class MovementValidator
    {

        public interface IValidationTarget
        {

            void Validated(int _step, in InputStep _input, in SimulationStep _simulation);

        }

        public interface ICorrectionTarget
        {

            void Corrected(int _step, in SimulationStepInfo _simulation);
            void Rejected(int _step, bool _newer);

        }

        public int Step { get; private set; }
        public bool IsRunning { get; private set; }

        public IValidationTarget validationTarget;
        public ICorrectionTarget correctionTarget;

        private struct Node
        {
            public InputStep? input;
            public SimulationStep? simulation;
        }

        private readonly Node[] m_buffer;
        private int m_Length => m_buffer.Length;
        private SimulationStepInfo m_last;
        private int m_trustedSteps;
        private int m_maxTrustedSteps;

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

        private int GetStep(int _step)
        {
            return _step % m_Length;
        }

        public MovementValidator(float _duration)
        {
            m_buffer = new Node[TimeStep.GetStepsInPeriod(_duration)];
            MaxTrustedSteps = 2;
        }

        public void SendCorrection()
        {
            m_trustedSteps = 0;
            correctionTarget?.Corrected(Step, m_last);
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
            m_last.simulation = _simulation;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Put(int _firstStep, IEnumerable<InputStep> _inputSteps, in SimulationStep _simulation)
        {
            int step = _firstStep;
            foreach (InputStep inputStep in _inputSteps)
            {
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
                    m_buffer[GetStep(step)].input = inputStep;
                }
                step++;
            }
            if (step < Step + m_Length)
            {
                m_buffer[GetStep(step - 1)].simulation = _simulation;
            }
        }

        private void Validate()
        {
            int bufInd = GetStep(Step);
            m_last.input = m_buffer[bufInd].input ?? m_last.input.Predicted;
            m_last.simulation = m_last.simulation.Simulate(m_last.input, TimeStep.c_simulationStep);
            validationTarget?.Validated(Step, m_last.input, m_last.simulation);
            if (m_buffer[bufInd].simulation != null)
            {
                m_trustedSteps = 0;
                if (!SimulationStep.IsNearlyEqual(m_last.simulation, m_buffer[bufInd].simulation.Value))
                {
                    Debugging.Printer.DebugIncrement("WrongData");
                    SendCorrection();
                }
            }
            else
            {
                m_trustedSteps++;
                if (m_trustedSteps > m_maxTrustedSteps)
                {
                    Debugging.Printer.DebugIncrement("NoData");
                    SendCorrection();
                }
            }
            m_buffer[bufInd] = new Node();
            Step++;
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

    }

}
