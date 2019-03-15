#define ENABLE_PARTIAL_SIMULATION

using UnityEngine;

namespace Wheeled.Gameplay.Movement
{

    internal sealed partial class MovementController
    {

        private SimulationStep m_lastSimulation;
        private readonly History m_history;
        private InputStep m_accumulatedInput;
        private float m_accumulatedTime;
        private Snapshot m_snapshot;
        private int m_oldestInputStep;
        private InputStep[] m_inputBuffer;

        public bool isPartialSimulationEnabled;

        public int InputBufferSize
        {
            get => m_inputBuffer?.Length ?? 0;
            set
            {
                Debug.Assert(value > 0);
                if (value != InputBufferSize)
                {
                    InputStep[] newBuffer = new InputStep[value];
                    if (m_oldestInputStep != -1)
                    {
                        m_oldestInputStep = Mathf.Max(m_oldestInputStep, Time.Step - value + 1);
                        for (int i = m_oldestInputStep; i <= Time.Step; i++)
                        {
                            newBuffer[i % value] = m_inputBuffer[i % m_inputBuffer.Length];
                        }
                    }
                    m_inputBuffer = newBuffer;
                }
            }
        }

        public TimeStep Time { get; private set; }
        public bool IsRunning { get; private set; }
        public Snapshot RawSnapshot => m_snapshot;
        public Snapshot ViewSnapshot { get; private set; }

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
                input.movementX /= m_accumulatedTime;
                input.movementZ /= m_accumulatedTime;
            }
            ClampMovement(ref input.movementX, ref input.movementZ);
            return input;
        }

        private void CommitInput()
        {
            if (m_oldestInputStep == -1)
            {
                m_oldestInputStep = Time.Step;
            }
            else
            {
                m_oldestInputStep = Mathf.Max(m_oldestInputStep, Time.Step - InputBufferSize + 1);
            }
            InputStep input = GetAccumulatedInput();
            m_inputBuffer[Time.Step % InputBufferSize] = input;
            m_lastSimulation = m_snapshot.simulation;
            m_snapshot.simulation = m_snapshot.simulation.Simulate(input, TimeStep.c_simulationStep);
            m_history.Append(Time.Step, new SimulationStepInfo { input = input, simulation = m_snapshot.simulation });
            m_accumulatedInput = new InputStep();
            m_accumulatedTime = 0.0f;
        }

        private void ProcessInput(TimeStep _now)
        {
            float processDeltaTime = (_now - Time).Seconds;
            TimeStep lastProcessTime = Time;

            float right = Input.GetAxis("Horizontal");
            float forward = Input.GetAxis("Vertical");
            bool jumped = Input.GetButtonDown("Jump");
            bool dashed = false;
            float turn = Input.GetAxis("Mouse X");
            float lookUp = -Input.GetAxis("Mouse Y");

            while (Time < _now)
            {
                TimeStep step = TimeStep.Min(Time.Next, _now);
                float stepDeltaTime = (step - Time).Seconds;

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
                Time = step;
                if (!step.HasRemainder)
                {
                    CommitInput();
                }
            }
        }

        private void UpdateView()
        {
            Snapshot viewSnapshot = m_snapshot;
            if (isPartialSimulationEnabled)
            {
                viewSnapshot.simulation = viewSnapshot.simulation.Simulate(GetAccumulatedInput(), m_accumulatedTime);
            }
            else
            {
                viewSnapshot.simulation = SimulationStep.Lerp(m_lastSimulation, viewSnapshot.simulation, m_accumulatedTime / TimeStep.c_simulationStep);
            }
            ViewSnapshot = viewSnapshot;
        }

        public MovementController(float _historyDuration)
        {
            m_history = new History(_historyDuration);
            m_oldestInputStep = -1;
            InputBufferSize = 10;
            isPartialSimulationEnabled = true;
        }

        public void StartAt(TimeStep _time)
        {
            ClearInputBuffer();
            Time = _time;
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
            m_history.Cut(RoomTime.Now.Step);
            UpdateView();
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void PullInputBuffer(InputStep[] _target, out int _outCount, int _maxSteps)
        {
            int step = Mathf.Max(Time.Step - _maxSteps + 1, m_oldestInputStep, 0);
            int i = 0;
            while (step < Time.Step)
            {
                _target[i++] = m_inputBuffer[step % InputBufferSize];
            }
            _outCount = i;
        }

        public void PullInputBuffer(InputStep[] _target, out int _outCount)
        {
            PullInputBuffer(_target, out _outCount, _target.Length);
        }

        public void UpdateUntil(TimeStep _time)
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
            SimulationStep? correctedSimulation = m_history.Correct(_step, _simulation);
            if (correctedSimulation != null)
            {
                m_snapshot.simulation = correctedSimulation.Value;
            }
            UpdateView();
        }

    }

}
