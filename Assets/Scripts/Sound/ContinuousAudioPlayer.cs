using UnityEngine;

namespace Wheeled.Sound
{

    public sealed class ContinuousAudioPlayer : AudioPlayer
    {

        public float value;

        public bool useDelta;

        [Range(0.0f, 2.0f)]
        public float smoothTime = 1.0f;

        public float maxSpeed = float.PositiveInfinity;

        private float m_value = 0.0f;
        private float m_velocity = 0.0f;
        private float m_lastValue = 0.0f;

        public void ReachValue()
        {
            m_value = value;
            m_velocity = 0.0f;
            m_lastValue = value;
        }

        protected override void AudioReady()
        {
            Setup();
        }

        private void OnEnable()
        {
            Setup();   
        }

        private void Setup()
        {
            ReachValue();
            SetValue(m_value);
            StartPlaying(true);
        }

        private void Update()
        {
            float target = useDelta ? Mathf.Abs(value - m_lastValue) / Time.deltaTime : value;
            m_value = Mathf.SmoothDamp(m_value, valueRange.Clamp(target), ref m_velocity, smoothTime, maxSpeed, Time.deltaTime);
            m_lastValue = value;
            SetValue(m_value);
        }

    }

}
