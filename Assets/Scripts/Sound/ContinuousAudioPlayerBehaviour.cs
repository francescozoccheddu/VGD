using UnityEditor;
using UnityEngine;

namespace Wheeled.Sound
{

    public sealed class ContinuousAudioPlayerBehaviour : AudioPlayerBehaviour
    {

        public float value;

        public bool useDelta;

        [Range(0.0f, 0.5f)]
        public float attackSmoothTime = 0.1f;

        [Range(0.0f, 0.5f)]
        public float releaseSmoothTime = 0.1f;

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

        protected override void AudioReady() => Setup();

        private void OnEnable() => Setup();

        private void Setup()
        {
            ReachValue();
            SetValue(m_value);
            StartPlaying(true);
        }

        private void Update()
        {
            float target = useDelta ? Mathf.Abs(value - m_lastValue) / Time.deltaTime : value;
            float smoothTime = target > m_value ? attackSmoothTime : releaseSmoothTime;
            m_value = Mathf.SmoothDamp(m_value, valueRange.Clamp(target), ref m_velocity, smoothTime, maxSpeed, Time.deltaTime);
            m_lastValue = value;
            SetValue(m_value);
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ContinuousAudioPlayerBehaviour))]
    public sealed class ContinuousAudioPlayerEditor : AudioPlayerEditor
    {

        private float m_value;
        private bool m_override;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                m_override = EditorGUILayout.Toggle("Override value", m_override);
                if (m_override)
                {
                    ContinuousAudioPlayerBehaviour audioPlayer = (ContinuousAudioPlayerBehaviour) target;
                    m_value = EditorGUILayout.Slider(m_value, audioPlayer.valueRange.min, audioPlayer.valueRange.max);
                    audioPlayer.value = m_value;
                }
            }
        }

    }

#endif


}
