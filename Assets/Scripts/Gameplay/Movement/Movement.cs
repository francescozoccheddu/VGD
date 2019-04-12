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
        public Vector3 Direction => Quaternion * Vector3.forward;

        public float LookUp
        {
            get => m_lookUp;
            set => m_lookUp = Mathf.Clamp(value, -40.0f, 80.0f);
        }

        public Quaternion Quaternion => Quaternion.Euler(m_lookUp, m_turn, 0.0f);

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

    internal struct SimulationStepInfo
    {
        public InputStep input;
        public CharacterController simulation;
    }

    internal struct Snapshot
    {
        public Sight sight;
        public CharacterController simulation;

        public static Snapshot Lerp(in Snapshot _a, in Snapshot _b, float _progress)
        {
            Snapshot l;
            l.simulation = CharacterController.Lerp(_a.simulation, _b.simulation, _progress);
            l.sight = Sight.Lerp(_a.sight, _b.sight, _progress);
            return l;
        }
    }
}