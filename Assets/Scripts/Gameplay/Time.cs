﻿using System;
using UnityEngine;

namespace Wheeled.Gameplay
{

    internal struct TimeStep : IEquatable<TimeStep>, IComparable<TimeStep>, IComparable
    {

        public static int GetStepsInPeriod(float _seconds)
        {
            return Mathf.CeilToInt(_seconds / c_simulationStep);
        }

        public static TimeStep FromSeconds(float _seconds)
        {
            return new TimeStep(0, _seconds);
        }

        public static TimeStep FromSteps(int _steps)
        {
            return new TimeStep(_steps, 0.0f);
        }

        public const float c_simulationStep = 1 / 30.0f;

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

        #region Operators

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

        #endregion

        #region Interfaces

        bool IEquatable<TimeStep>.Equals(TimeStep _other)
        {
            return this == _other;
        }

        int IComparable<TimeStep>.CompareTo(TimeStep _other)
        {
            if (this < _other)
            {
                return -1;
            }
            else if (this > _other)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        int IComparable.CompareTo(object _obj)
        {
            if (_obj is TimeStep other)
            {
                return ((IComparable) this).CompareTo(other);
            }
            else
            {
                return 1;
            }

        }

        #endregion

        #region Object overrides

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

        public override string ToString()
        {
            return string.Format("{0}-{1:F3}", Step, m_remainder);
        }

        #endregion

    }

    internal static class RoomTime
    {

        public static class Manager
        {

            private static float s_offset;
            private static float s_smoothTime;
            private static float s_smoothVelocity;
            private static bool s_isInterpolating;

            public static TimeStep Target => s_isInterpolating ? Now + s_offset : Now;

            public static void Stop()
            {
                IsRunning = false;
            }

            public static void Start()
            {
                IsRunning = true;
            }

            public static void Set(TimeStep _time, bool _interpolate)
            {
                if (_interpolate)
                {
                    s_offset = (_time - Now).Seconds;
                    s_smoothTime = Mathf.Log10(Mathf.Abs(s_offset) + 2);
                    s_smoothVelocity = 0.0f;
                    s_isInterpolating = true;
                }
                else
                {
                    s_isInterpolating = false;
                    Now = _time;
                }
            }

            public static void Update()
            {
                if (IsRunning)
                {
                    Now += UnityEngine.Time.deltaTime;
                }
                if (s_isInterpolating)
                {
                    float oldOffset = s_offset;
                    s_offset = Mathf.SmoothDamp(s_offset, 0.0f, ref s_smoothVelocity, s_smoothTime);
                    float step = oldOffset - s_offset;
                    Now += step;
                    if (Mathf.Approximately(s_offset, 0.0f))
                    {
                        s_isInterpolating = false;
                    }
                }
            }

        }

        public static TimeStep Now { get; private set; }

        public static bool IsRunning { get; private set; }

    }

}
