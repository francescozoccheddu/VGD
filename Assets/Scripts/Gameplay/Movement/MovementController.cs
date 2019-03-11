#define ENABLE_PARTIAL_SIMULATION

using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Networking;

namespace Wheeled.Gameplay.Movement
{

    internal sealed class MovementController : IUpdatable
    {

        public interface IFlushTarget
        {

            void Flush(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot);

        }

#if !ENABLE_PARTIAL_SIMULATION
        private SimulationStep m_lastSimulationStep;
#endif

        private InputStep m_accumulatedInput;
        private float m_accumulatedTime;
        private Snapshot m_snapshot;

        public IFlushTarget target;

        public int FlushRate
        {
            get => m_inputSteps.Length;
            set => SetFlushRate(value);
        }

        public TimeStep Offset { get; private set; } = TimeStep.zero;

        public TimeStep LastCommitTime { get; private set; }

        public bool IsRunning { get; private set; }

        public Snapshot ViewSnapshot { get; private set; }

        public static void ClampMovement(ref float _refX, ref float _refZ)
        {
            float length = Mathf.Sqrt(_refX * _refX + _refZ * _refZ);
            if (length > 1.0f)
            {
                _refX /= length;
                _refZ /= length;
            }
        }

        public static void RotateMovementXZ(float _right, float _forward, float _turn, out float _outX, out float _outZ)
        {
            float angleRad = Mathf.Deg2Rad * _turn;
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            _outX = (cos * _right) + (sin * _forward);
            _outZ = (cos * _forward) - (sin * _right);
        }

        private int m_inputStepCount = 0;
        private InputStep[] m_inputSteps;
        private int m_firstStep;

        private InputStep GetAccumulatedInput()
        {
            InputStep input = m_accumulatedInput;
            if (m_accumulatedTime > 0.0f)
            {
                input.movementX /= m_accumulatedTime;
                input.movementZ /= m_accumulatedTime;
            }
            ClampMovement(ref input.movementX, ref input.movementZ);
            return input;
        }

        private void CommitInput(int _step)
        {
            if (m_inputStepCount == 0)
            {
                m_firstStep = _step;
            }
            else if (_step - m_inputStepCount != m_firstStep)
            {
                Flush();
                m_firstStep = _step;
            }
            InputStep input = GetAccumulatedInput();
            m_inputSteps[m_inputStepCount++] = input;
#if !ENABLE_PARTIAL_SIMULATION
            m_lastSimulationStep = m_snapshot.simulation;
#endif
            m_snapshot.simulation = m_snapshot.simulation.Simulate(input, TimeStep.c_simulationStep);
            if (m_inputStepCount >= m_inputSteps.Length)
            {
                Flush();
            }
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
        }

        private void ProcessInput()
        {
            TimeStep now = RoomTime.Now + Offset;
            float processDeltaTime = (now - LastCommitTime).Seconds;
            TimeStep lastProcessTime = LastCommitTime;

            float right = Input.GetAxis("Horizontal");
            float forward = Input.GetAxis("Vertical");
            bool jumped = Input.GetButtonDown("Jump");
            bool dashed = false;
            float turn = Input.GetAxis("Mouse X");
            float lookUp = -Input.GetAxis("Mouse Y");

            while (LastCommitTime < now)
            {
                TimeStep step = TimeStep.Min(LastCommitTime.Next, now);
                float stepDeltaTime = (step - LastCommitTime).Seconds;
                RotateMovementXZ(right, forward, m_snapshot.sight.Turn, out float movementX, out float movementZ);
                ClampMovement(ref movementX, ref movementZ);
                m_accumulatedInput.movementX += movementX * stepDeltaTime;
                m_accumulatedInput.movementZ += movementZ * stepDeltaTime;
                m_accumulatedInput.jump |= jumped;
                m_accumulatedInput.dash |= dashed;
                jumped = false;
                dashed = false;
                float weight = stepDeltaTime / processDeltaTime;
                m_snapshot.sight.Turn += turn * weight;
                m_snapshot.sight.LookUp += lookUp * weight;
                m_accumulatedTime += stepDeltaTime;
                if (!step.HasRemainder)
                {
                    CommitInput(step.Step);
                }
                LastCommitTime = step;
            }
        }

        private void UpdateView()
        {
            Snapshot viewSnapshot = m_snapshot;
#if ENABLE_PARTIAL_SIMULATION
            viewSnapshot.simulation = viewSnapshot.simulation.Simulate(GetAccumulatedInput(), m_accumulatedTime);
#else
            viewSnapshot.simulation = SimulationStep.Lerp(m_lastSimulationStep, viewSnapshot.simulation, m_accumulatedTime / TimeStep.c_simulationStep);
#endif
            ViewSnapshot = viewSnapshot;
        }

        private void SetFlushRate(int _flushRate)
        {
            Debug.Assert(_flushRate > 0);
            if (m_inputSteps != null && _flushRate < m_inputStepCount)
            {
                Flush();
            }
            if (m_inputSteps == null || _flushRate != m_inputSteps.Length)
            {
                InputStep[] newBuffer = new InputStep[_flushRate];
                if (m_inputSteps != null)
                {
                    Array.Copy(m_inputSteps, newBuffer, m_inputStepCount);
                }
                m_inputSteps = newBuffer;
            }
        }

        public MovementController()
        {
            SetFlushRate(1);
        }

        public void Flush()
        {
            if (m_inputStepCount > 0)
            {
                target?.Flush(m_firstStep, new ArraySegment<InputStep>(m_inputSteps, 0, m_inputStepCount), m_snapshot);
            }
            m_inputStepCount = 0;
        }

        public void StartAt(TimeStep _time, TimeStep _offset, bool _flushPending = true)
        {
            if (_flushPending)
            {
                Flush();
            }
            m_inputStepCount = 0;
            Offset = _offset;
            LastCommitTime = _time;
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
            IsRunning = true;
        }

        public void Teleport(Snapshot _snapshot, bool _resetInput = false)
        {
            m_snapshot = _snapshot;
#if !ENABLE_PARTIAL_SIMULATION
            m_lastSimulationStep = _snapshot.simulation;
#endif
            if (_resetInput)
            {
                m_accumulatedInput = new InputStep();
                m_accumulatedTime = 0.0f;
            }
            UpdateView();
        }

        public void Pause(bool _flushPending = true)
        {
            if (_flushPending)
            {
                Flush();
            }
            m_inputStepCount = 0;
            IsRunning = false;
        }

        public void Update()
        {
            if (IsRunning)
            {
                ProcessInput();
            }
            UpdateView();
        }

    }

}
