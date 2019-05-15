using System;
using UnityEngine;

namespace Wheeled.Gameplay
{
    public static class TimeConstants
    {
        public const double c_simulationStep = 1 / 30.0f;

        public static int CeilingSimulationSteps(this double _period)
        {
            return (int) Math.Ceiling(_period / c_simulationStep);
        }

        public static double SimulationPeriod(this int _step)
        {
            return _step * c_simulationStep;
        }

        public static int SimulationSteps(this double _period)
        {
            return (int) Math.Floor(_period / c_simulationStep);
        }

        public static double Lerp(double _a, double _b, float _alpha)
        {
            return _a * (1.0 - _alpha) + _b * _alpha;
        }

        public static double Smooth(double _current, double _target, float _deltaTime, float _smoothSpeed, float _maxSpeed = 0.5f)
        {
            return Lerp(_current, _target, Mathf.Clamp01(Math.Min(_deltaTime * _smoothSpeed, _maxSpeed)));
        }

        public sealed class Tapper
        {

            private float m_lastTapTime = float.NaN;
            public float AverageInterval { get; set; }
            public float SmoothQuickness { get; set; }

            public Tapper(float _initialAverageInterval)
            {
                AverageInterval = _initialAverageInterval;
                SmoothQuickness = 0.5f;
            }

            public void Tap()
            {
                float currentTime = Time.unscaledTime;
                if (!float.IsNaN(m_lastTapTime))
                {
                    AverageInterval = Mathf.Lerp(AverageInterval, currentTime - m_lastTapTime, SmoothQuickness);
                }
                m_lastTapTime = currentTime;
            }

            public void QuietTap()
            {
                m_lastTapTime = Time.unscaledTime;
            }

        }

    }
}