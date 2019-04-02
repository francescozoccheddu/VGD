using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    internal struct InputStep
    {
        public bool dash;
        public bool jump;
        public float movementX;
        public float movementZ;

        public InputStep Clamped
        {
            get
            {
                InputStep clamped = this;
                clamped.Clamp();
                return clamped;
            }
        }

        public InputStep Predicted
        {
            get
            {
                InputStep predicted = this;
                predicted.jump = false;
                predicted.dash = false;
                return predicted;
            }
        }

        public static bool IsNearlyEqual(in InputStep _a, in InputStep _b)
        {
            return IsNearlyEqual(_a.movementX, _b.movementX)
                && IsNearlyEqual(_a.movementZ, _b.movementZ)
                && _a.dash == _b.dash
                && _a.jump == _b.jump;
        }

        public void Clamp()
        {
            float length = Mathf.Sqrt(movementX * movementX + movementZ * movementZ);
            if (length > 1.0f)
            {
                movementX /= length;
                movementZ /= length;
            }
        }

        private static bool IsNearlyEqual(float _a, float _b)
        {
            return Mathf.Approximately(_a, _b);
        }
    }

    internal struct Sight
    {
        private float m_lookUp;
        private float m_turn;
        public Vector3 Direction => Quaternion.Euler(m_lookUp, m_turn, 0.0f) * Vector3.forward;

        public float LookUp
        {
            get => m_lookUp;
            set => m_lookUp = Mathf.Clamp(value, -40.0f, 80.0f);
        }

        public float Turn
        {
            get => m_turn;
            set
            {
                m_turn = value % 360.0f;
                if (m_turn < 0.0f)
                {
                    m_turn += 360.0f;
                }
            }
        }

        public static Sight Lerp(in Sight _a, in Sight _b, float _progress)
        {
            Sight l;
            l.m_turn = Mathf.LerpAngle(_a.Turn, _b.Turn, _progress);
            l.m_lookUp = Mathf.LerpAngle(_a.LookUp, _b.LookUp, _progress);
            return l;
        }
    }

    internal struct SimulationStep
    {
        public Vector3 position;
        public Vector3 velocity;
        private const float c_gravitySpeed = -10f;
        private const float c_jumpSpeed = 100;
        private const float c_moveSpeed = 5;

        public static bool IsNearlyEqual(in SimulationStep _a, in SimulationStep _b)
        {
            return IsNearlyEqual(_a.velocity, _b.velocity)
                && IsNearlyEqual(_a.position, _b.position);
        }

        public static SimulationStep Lerp(in SimulationStep _a, in SimulationStep _b, float _progress)
        {
            SimulationStep l;
            l.velocity = Vector3.Lerp(_a.velocity, _b.velocity, _progress);
            l.position = Vector3.Lerp(_a.position, _b.position, _progress);
            return l;
        }

        public SimulationStep Simulate(in InputStep _input, double _deltaTime)
        {
            SimulationStep next = this;
            InputStep input = _input.Clamped;
            next.velocity.x = input.movementX * c_moveSpeed;
            next.velocity.z = input.movementZ * c_moveSpeed;
            if (next.position.y == 2.0f)
            {
                if (input.jump)
                {
                    next.velocity.y = c_jumpSpeed;
                }
                else
                {
                    next.velocity.y = 0.0f;
                }
            }
            next.velocity.y += (float) (c_gravitySpeed * _deltaTime);
            next.position += velocity * (float) _deltaTime;
            if (next.position.y <= 2.0f)
            {
                next.position.y = 2.0f;
            }
            return next;
        }

        private static bool IsNearlyEqual(float _a, float _b)
        {
            return Mathf.Approximately(_a, _b);
        }

        private static bool IsNearlyEqual(in Vector3 _a, in Vector3 _b)
        {
            return IsNearlyEqual(_a.x, _b.x)
                && IsNearlyEqual(_a.y, _b.y)
                && IsNearlyEqual(_a.z, _b.z);
        }
    }

    internal struct SimulationStepInfo
    {
        public InputStep input;
        public SimulationStep simulation;
    }

    internal struct Snapshot
    {
        public Sight sight;
        public SimulationStep simulation;

        public static Snapshot Lerp(in Snapshot _a, in Snapshot _b, float _progress)
        {
            Snapshot l;
            l.simulation = SimulationStep.Lerp(_a.simulation, _b.simulation, _progress);
            l.sight = Sight.Lerp(_a.sight, _b.sight, _progress);
            return l;
        }
    }
}