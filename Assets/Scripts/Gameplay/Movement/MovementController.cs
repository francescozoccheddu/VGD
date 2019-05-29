#define ENABLE_PARTIAL_SIMULATION

using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    public sealed partial class MovementController
    {

        public bool EnableMovement { get; set; } = true;
        public bool EnableSight { get; set; } = true;
        public bool EnableDash { get; set; } = true;
        public bool EnableJump { get; set; } = true;


        public static float HorizontalMovement => Input.GetAxis("Horizontal");
        public static float VerticalMovement => Input.GetAxis("Vertical");
        public static float HorizontalSight => Input.GetAxis("Mouse X");
        public static float VerticalSight => Input.GetAxis("Mouse Y");

        public static bool IsJumping => Input.GetButtonDown("Jump");
        public static bool IsDashing => Input.GetButtonDown("Dash");


        public ICommitTarget target;

        private InputStep m_accumulatedInput;

        private double m_accumulatedTime;

        private CharacterController m_lastSimulation;

        private Snapshot m_snapshot;

        public interface ICommitTarget
        {
            void Commit(int _step, InputStep _input, Snapshot _snapshot);
        }

        public bool IsRunning { get; private set; }
        public Snapshot RawSnapshot => m_snapshot;
        public int Step { get; private set; }
        public double Time { get; private set; }

        public void Pause() => IsRunning = false;

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
        }

        public void UpdateUntil(double _time)
        {
            if (IsRunning)
            {
                ProcessInput(_time);
            }
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
            m_snapshot.simulation = m_snapshot.simulation.Simulate(input, (float) TimeConstants.c_simulationStep);
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

            float right = HorizontalMovement;
            float forward = VerticalMovement;
            bool jumped = IsJumping;
            bool dashed = IsDashing;
            float turn = HorizontalSight;
            float lookUp = -VerticalSight;

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
                if (EnableMovement)
                {
                    m_accumulatedInput.movementX += (float) (movementX * stepDeltaTime);
                    m_accumulatedInput.movementZ += (float) (movementZ * stepDeltaTime);
                }
                if (EnableJump)
                {
                    m_accumulatedInput.jump |= jumped;
                }
                if (EnableDash)
                {
                    m_accumulatedInput.dash |= dashed;
                }

                jumped = false;
                dashed = false;

                if (EnableSight)
                {
                    float weight = (float) (stepDeltaTime / processDeltaTime);
                    m_snapshot.sight.Turn += turn * weight;
                    m_snapshot.sight.LookUp += lookUp * weight;
                }

                m_accumulatedTime += stepDeltaTime;
                if (finalized)
                {
                    Step++;
                    CommitInput();
                }
            }
        }
    }
}