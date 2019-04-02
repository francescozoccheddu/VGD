#define ENABLE_PARTIAL_SIMULATION

using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    internal sealed partial class MovementController
    {
        public bool m_isPartialSimulationEnabled;

        public ICommitTarget target;

        private InputStep m_accumulatedInput;

        private double m_accumulatedTime;

        private SimulationStep m_lastSimulation;

        private Snapshot m_snapshot;

        public MovementController()
        {
            m_isPartialSimulationEnabled = true;
        }

        public interface ICommitTarget
        {
            void Commit(int _step, InputStep _input, Snapshot _snapshot);

            void Cut(int _oldest);
        }

        public bool IsPartialSimulationEnabled
        {
            get => m_isPartialSimulationEnabled;
            set
            {
                m_isPartialSimulationEnabled = value;
                UpdateView();
            }
        }

        public bool IsRunning { get; private set; }
        public Snapshot RawSnapshot => m_snapshot;
        public int Step { get; private set; }
        public double Time { get; private set; }
        public Snapshot ViewSnapshot { get; private set; }

        public void Pause()
        {
            IsRunning = false;
        }

        public void StartAt(double _time)
        {
            Time = _time;
            Step = _time.SimulationSteps();
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
            IsRunning = true;
        }

        public void Teleport(Snapshot _snapshot, bool _resetInput = false)
        {
            m_snapshot = _snapshot;
            m_lastSimulation = _snapshot.simulation;
            if (_resetInput)
            {
                m_accumulatedInput = new InputStep();
                m_accumulatedTime = 0.0f;
            }
            UpdateView();
        }

        public void UpdateUntil(double _time)
        {
            if (IsRunning)
            {
                ProcessInput(_time);
            }
            UpdateView();
        }

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

        private void CommitInput()
        {
            InputStep input = GetAccumulatedInput();
            m_lastSimulation = m_snapshot.simulation;
            m_snapshot.simulation = m_snapshot.simulation.Simulate(input, TimeConstants.c_simulationStep);
            target?.Commit(Step, input, m_snapshot);
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
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
    }
}