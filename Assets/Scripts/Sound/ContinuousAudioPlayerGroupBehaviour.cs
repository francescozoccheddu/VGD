using UnityEditor;
using UnityEngine;

namespace Wheeled.Sound
{

    public sealed class ContinuousAudioPlayerGroupBehaviour : MonoBehaviour
    {

        public float value;

        public ContinuousAudioPlayerBehaviour[] audioPlayers;

        public void ReachValue()
        {
            foreach (var audioPlayer in audioPlayers)
            {
                audioPlayer.ReachValue();
            }
        }

        private void Update()
        {
            foreach (var audioPlayer in audioPlayers)
            {
                audioPlayer.value = value;
            }
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ContinuousAudioPlayerGroupBehaviour))]
    public sealed class ContinuousAudioPlayerGroupEditor : Editor
    {

        private float m_value;
        private bool m_override;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Test", EditorStyles.boldLabel);
                if (GUILayout.Button("Recreate"))
                {
                    ((AudioPlayerBehaviour) target).Recreate();
                }
                m_override = EditorGUILayout.Toggle("Override value", m_override);
                if (m_override)
                {
                    ContinuousAudioPlayerGroupBehaviour audioPlayer = (ContinuousAudioPlayerGroupBehaviour) target;
                    m_value = EditorGUILayout.FloatField(m_value);
                    audioPlayer.value = m_value;
                }
            }
        }

    }

#endif


}
