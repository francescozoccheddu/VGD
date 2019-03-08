using UnityEngine;

namespace Wheeled.Networking
{

    internal struct TimeStep
    {

        public const float c_simulationStep = 1 / 60.0f;
        public const int c_minStepsPerCommit = 2;
        public const int c_maxStepsPerCommit = 6;

        public static readonly TimeStep zero = new TimeStep(0, 0.0f);

        private float m_remainder;

        public float Seconds => Step * c_simulationStep + m_remainder;

        public int Step { get; set; }

        public TimeStep Floor => new TimeStep(Step, 0.0f);

        public TimeStep Next => new TimeStep(Step + 1, 0.0f);

        public TimeStep Ceil => m_remainder == 0.0f ? this : Next;

        public bool HasRemainder => Remainder != 0.0f;

        public float Remainder
        {
            get => m_remainder;
            set
            {
                m_remainder = value;
                Commit();
            }
        }

        public TimeStep(int _node, float _timeSinceNode) : this()
        {
            Step = _node;
            m_remainder = _timeSinceNode;
            Commit();
        }

        private void Commit()
        {
            Step += Mathf.FloorToInt(m_remainder / c_simulationStep);
            m_remainder %= c_simulationStep;
            if (m_remainder < 0.0f)
            {
                m_remainder = c_simulationStep - m_remainder;
            }
        }

        public override bool Equals(object _obj)
        {
            if (!(_obj is TimeStep))
            {
                return false;
            }

            TimeStep other = (TimeStep) _obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            int hashCode = -1322611433;
            hashCode = hashCode * -1521134295 + Step.GetHashCode();
            hashCode = hashCode * -1521134295 + m_remainder.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(TimeStep _a, TimeStep _b)
        {
            return _a.Step == _b.Step && _a.m_remainder == _b.m_remainder;
        }

        public static bool operator !=(TimeStep _a, TimeStep _b)
        {
            return !(_a == _b);
        }

        public static TimeStep operator +(TimeStep _a, TimeStep _b)
        {
            return new TimeStep(
                _a.Step + _b.Step,
                _a.m_remainder + _b.m_remainder
            );
        }

        public static TimeStep operator -(TimeStep _a)
        {
            return new TimeStep
            (
                -_a.Step,
                -_a.m_remainder
            );
        }

        public static TimeStep operator -(TimeStep _a, TimeStep _b)
        {
            return _a + (-_b);
        }

        public static TimeStep operator +(TimeStep _a, float _b)
        {
            return _a + new TimeStep { m_remainder = _b };
        }

        public static TimeStep operator -(TimeStep _a, float _b)
        {
            return _a + (-_b);
        }

        public static TimeStep operator +(float _a, TimeStep _b)
        {
            return _b + _a;
        }

        public static TimeStep operator -(float _a, TimeStep _b)
        {
            return (-_b) + _a;
        }

        public static bool operator <(TimeStep _a, TimeStep _b)
        {
            if (_a.Step > _b.Step)
            {
                return false;
            }
            return _a.Step < _b.Step || _a.m_remainder < _b.m_remainder;
        }

        public static bool operator >(TimeStep _a, TimeStep _b)
        {
            if (_a.Step < _b.Step)
            {
                return false;
            }
            return _a.Step > _b.Step || _a.m_remainder > _b.m_remainder;
        }

        public static bool operator <=(TimeStep _a, TimeStep _b)
        {
            if (_a.Step > _b.Step)
            {
                return false;
            }
            return _a.Step < _b.Step || _a.m_remainder <= _b.m_remainder;
        }

        public static bool operator >=(TimeStep _a, TimeStep _b)
        {
            if (_a.Step < _b.Step)
            {
                return false;
            }
            return _a.Step > _b.Step || _a.m_remainder >= _b.m_remainder;
        }

        public static TimeStep Min(TimeStep _a, TimeStep _b)
        {
            return _a < _b ? _a : _b;
        }

        public static TimeStep Max(TimeStep _a, TimeStep _b)
        {
            return _a > _b ? _a : _b;
        }

    }

    internal static class RoomTime
    {

        public static TimeStep Time { get; }

        public static bool Running { get; }

    }

}
