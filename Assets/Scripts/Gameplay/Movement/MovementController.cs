#define ENABLE_PARTIAL_SIMULATION

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Gameplay.Movement
{

    internal sealed partial class MovementController
    {

        public interface IFlushTarget
        {

            void FlushCombined(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot);
            void FlushSimulation(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in SimulationStep _simulation);
            void FlushSight(int _step, in Sight _sight);

        }

#if !ENABLE_PARTIAL_SIMULATION
        private SimulationStep m_lastSimulationStep;
#endif

        private readonly History m_history;
        private InputStep m_accumulatedInput;
        private float m_accumulatedTime;
        private Snapshot m_snapshot;
        private int m_skippedSights;
        private int m_sightFlushRate;

        public IFlushTarget target;

        public int SimulationFlushRate
        {
            get => m_inputSteps.Length;
            set
            {
                Debug.Assert(value > 0);
                if (m_inputSteps != null && value <= m_inputStepCount)
                {
                    FlushSimulation();
                }
                if (m_inputSteps == null || value != m_inputSteps.Length)
                {
                    InputStep[] newBuffer = new InputStep[value];
                    if (m_inputSteps != null)
                    {
                        Array.Copy(m_inputSteps, newBuffer, m_inputStepCount);
                    }
                    m_inputSteps = newBuffer;
                }
            }
        }

        public int SightFlushRate
        {
            get => m_sightFlushRate;
            set
            {
                Debug.Assert(value > 0);
                if (value <= m_skippedSights)
                {
                    FlushSight();
                }
                m_sightFlushRate = value;
                if (m_sightFlushRate == SimulationFlushRate)
                {
                    m_skippedSights = 0;
                }
            }
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
            // Simulation
            {
                if (m_inputStepCount == 0)
                {
                    m_firstStep = _step;
                }
                else if (_step - m_inputStepCount != m_firstStep)
                {
                    FlushSimulation();
                    m_firstStep = _step;
                }
                InputStep input = GetAccumulatedInput();
                m_inputSteps[m_inputStepCount++] = input;
#if !ENABLE_PARTIAL_SIMULATION
            m_lastSimulationStep = m_snapshot.simulation;
#endif
                m_snapshot.simulation = m_snapshot.simulation.Simulate(input, TimeStep.c_simulationStep);
                m_history.Append(_step, new SimulationStepInfo { input = input, simulation = m_snapshot.simulation });
                if (m_inputStepCount >= m_inputSteps.Length)
                {
                    if (SimulationFlushRate == SightFlushRate)
                    {
                        FlushCombined();
                    }
                    else
                    {
                        FlushSimulation();
                    }
                }
                m_accumulatedInput = new InputStep();
                m_accumulatedTime = 0.0f;
            }
            // Sight
            if (SimulationFlushRate != SightFlushRate)
            {
                m_skippedSights++;
                if (m_skippedSights > m_sightFlushRate)
                {
                    FlushSight();
                }
            }
        }

        public void FlushCombined()
        {
            if (m_inputStepCount > 0)
            {
                target?.FlushCombined(m_firstStep, new ArraySegment<InputStep>(m_inputSteps, 0, m_inputStepCount), m_snapshot);
                m_inputStepCount = 0;
                m_skippedSights = 0;
            }
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

        public MovementController(float _historyDuration)
        {
            m_history = new History(_historyDuration);
            SimulationFlushRate = 2;
            SightFlushRate = 2;
        }

        public void FlushSimulation()
        {
            if (m_inputStepCount > 0)
            {
                target?.FlushSimulation(m_firstStep, new ArraySegment<InputStep>(m_inputSteps, 0, m_inputStepCount), m_snapshot.simulation);
            }
            m_inputStepCount = 0;
        }

        public void FlushSight()
        {
            m_skippedSights = 0;
            target?.FlushSight(LastCommitTime.Step, m_snapshot.sight);
        }

        public void StartAt(TimeStep _time, TimeStep _offset, bool _flushPending = true)
        {
            if (_flushPending)
            {
                FlushSimulation();
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
            m_history.Cut(RoomTime.Now.Step);
            UpdateView();
        }

        public void Pause(bool _flushPending = true)
        {
            if (_flushPending)
            {
                FlushSimulation();
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

        public void Correct(int _step, SimulationStepInfo _simulation)
        {
            SimulationStep? correctedSimulation = m_history.Correct(_step, _simulation);
            if (correctedSimulation != null)
            {
                m_snapshot.simulation = correctedSimulation.Value;
            }
            UpdateView();
        }

    }

}
