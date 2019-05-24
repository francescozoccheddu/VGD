using UnityEditor;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Sound
{

    public sealed class OneShotAudioPlayerBehaviour : AudioPlayerBehaviour
    {

        public enum EAutoPlayMode
        {
            None, PlayOnAwake, PlayOnAwakeThenDestroy
        }

        public EAutoPlayMode autoPlayMode;

        protected override void AudioReady()
        {
            if (autoPlayMode != EAutoPlayMode.None)
            {
                Play();
            }
        }

        private void Update()
        {
            if (autoPlayMode == EAutoPlayMode.PlayOnAwakeThenDestroy && !IsPlaying)
            {
                Destroy(gameObject);
            }
        }

        public void Play()
        {
            Play(valueRange.max);
        }

        public void Play(float _value)
        {
            SetValue(_value);
            StartPlaying(false);
        }

    }


#if UNITY_EDITOR

    [CustomEditor(typeof(OneShotAudioPlayerBehaviour))]
    public sealed class OneShotAudioPlayerEditor : AudioPlayerEditor
    {

        private float m_value;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Recreate"))
                {
                    ((AudioPlayerBehaviour) target).Recreate();
                }
                OneShotAudioPlayerBehaviour audioPlayer = (OneShotAudioPlayerBehaviour) target;
                m_value = EditorGUILayout.Slider(m_value, audioPlayer.valueRange.min, audioPlayer.valueRange.max);
                if (GUILayout.Button("Play"))
                {
                    audioPlayer.Play(m_value);
                }
            }
        }

    }

#endif


}
