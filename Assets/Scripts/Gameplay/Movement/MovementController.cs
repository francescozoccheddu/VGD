﻿#define ENABLE_PARTIAL_SIMULATION

using UnityEngine;

namespace Wheeled.Gameplay.Movement
{

    internal sealed partial class MovementController
    {

        private SimulationStep m_lastSimulation;
        private readonly History m_history;
        private InputStep m_accumulatedInput;
        private double m_accumulatedTime;
        private Snapshot m_snapshot;
        private int m_oldestInputStep;

        public bool m_isPartialSimulationEnabled;

        public bool IsPartialSimulationEnabled
        {
            get => m_isPartialSimulationEnabled;
            set
            {
                m_isPartialSimulationEnabled = value;
                UpdateView();
            }
        }

        public double Time { get; private set; }
        public int Step { get; private set; }
        public bool IsRunning { get; private set; }
        public Snapshot RawSnapshot => m_snapshot;
        public Snapshot ViewSnapshot { get; private set; }
        public int HistoryLength => m_history.Length;

        private static void ClampMovement(ref float _refX, ref float _refZ)
        {
            float length = Mathf.Sqrt(_refX * _refX + _refZ * _refZ);
            if (length > 1.0f)
            {
                _refX /= length;
                _refZ /= length;
            }
        }

        private static void RotateMovementXZ(float _right, float _forward, float _turn, out float _outX, out float _outZ)
        {
            float angleRad = Mathf.Deg2Rad * _turn;
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            _outX = (cos * _right) + (sin * _forward);
            _outZ = (cos * _forward) - (sin * _right);
        }

        private InputStep GetAccumulatedInput()
        {
            InputStep input = m_accumulatedInput;
            if (m_accumulatedTime > 0.0f)
            {
                input.movementX = (float) (input.movementX / m_accumulatedTime);
                input.movementZ = (float) (input.movementZ / m_accumulatedTime);
            }
            ClampMovement(ref input.movementX, ref input.movementZ);
            return input;
        }

        private void CommitInput()
        {
            if (m_oldestInputStep == -1)
            {
                m_oldestInputStep = Step;
            }
            InputStep input = GetAccumulatedInput();
            m_lastSimulation = m_snapshot.simulation;
            m_snapshot.simulation = m_snapshot.simulation.Simulate(input, TimeConstants.c_simulationStep);
            m_history.Append(Step, input);
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
        }

        private void ProcessInput(double _now)
        {
            double processDeltaTime = _now - Time;

            float right = Input.GetAxis("Horizontal");
            float forward = Input.GetAxis("Vertical");
            bool jumped = Input.GetButtonDown("Jump");
            bool dashed = false;
            float turn = Input.GetAxis("Mouse X");
            float lookUp = -Input.GetAxis("Mouse Y");

            while (Time < _now)
            {
                double stepDeltaTime;
                bool finalized;
                {
                    double nextStep = (Step + 1).SimulationPeriod();
                    finalized = nextStep <= _now;
                    double newTime = finalized ? nextStep : _now;
                    stepDeltaTime = newTime - Time;
                    Time = newTime;
                }

                RotateMovementXZ(right, forward, m_snapshot.sight.Turn, out float movementX, out float movementZ);
                ClampMovement(ref movementX, ref movementZ);
                m_accumulatedInput.movementX += (float) (movementX * stepDeltaTime);
                m_accumulatedInput.movementZ += (float) (movementZ * stepDeltaTime);
                m_accumulatedInput.jump |= jumped;
                m_accumulatedInput.dash |= dashed;

                jumped = false;
                dashed = false;

                float weight = (float) (stepDeltaTime / processDeltaTime);
                m_snapshot.sight.Turn += turn * weight;
                m_snapshot.sight.LookUp += lookUp * weight;

                m_accumulatedTime += stepDeltaTime;
                if (finalized)
                {
                    Step++;
                    CommitInput();
                }
            }
        }

        private void UpdateView()
        {
            Snapshot viewSnapshot = m_snapshot;
            if (m_isPartialSimulationEnabled)
            {
                viewSnapshot.simulation = viewSnapshot.simulation.Simulate(GetAccumulatedInput(), m_accumulatedTime);
            }
            else
            {
                viewSnapshot.simulation = SimulationStep.Lerp(m_lastSimulation, viewSnapshot.simulation, (float) (m_accumulatedTime / TimeConstants.c_simulationStep));
            }
            ViewSnapshot = viewSnapshot;
        }

        public MovementController(int _historyLength)
        {
            m_history = new History(_historyLength);
            m_isPartialSimulationEnabled = true;
            m_oldestInputStep = -1;
        }

        public void StartAt(double _time)
        {
            ClearInputBuffer();
            Time = _time;
            Step = _time.SimulationSteps();
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
            IsRunning = true;
        }

        public void Teleport(Snapshot _snapshot, bool _resetInput = false, bool _clearInputBuffer = false)
        {
            m_snapshot = _snapshot;
            m_lastSimulation = _snapshot.simulation;
            if (_resetInput)
            {
                m_accumulatedInput = new InputStep();
                m_accumulatedTime = 0.0f;
            }
            if (_clearInputBuffer)
            {
                ClearInputBuffer();
            }
            m_history.Cut(Time.SimulationSteps());
            UpdateView();
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void PullReversedInputBuffer(InputStep[] _target, out int _outCount)
        {
            _outCount = 0;
            if (m_oldestInputStep != -1)
            {
                InputStep? node = m_history.Get(Step);
                while (node != null && Step - _outCount >= m_oldestInputStep && _outCount < _target.Length)
                {
                    _target[_outCount++] = node.Value;
                    node = m_history.Get(Step - _outCount);
                }
            }
        }

        public void UpdateUntil(double _time)
        {
            if (IsRunning)
            {
                ProcessInput(_time);
            }
            UpdateView();
        }

        public void ClearInputBuffer()
        {
            m_oldestInputStep = -1;
        }

        public void Correct(int _step, SimulationStepInfo _simulation)
        {
            if (_step > m_oldestInputStep)
            {
                m_oldestInputStep = _step + 1;
            }
            SimulationStep? correctedSimulation = m_history.Correct(_step, _simulation);
            if (correctedSimulation != null)
            {
                m_snapshot.simulation = correctedSimulation.Value;
            }
            UpdateView();
        }

    }

}
