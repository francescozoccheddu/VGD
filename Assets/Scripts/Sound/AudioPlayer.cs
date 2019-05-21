using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Sound
{

    public abstract class AudioPlayer : MonoBehaviour
    {

#pragma warning disable CA2235

        [System.Serializable]
        public struct Layer
        {

#if UNITY_EDITOR
            [HideInInspector]
            public string name;

            public static Layer MakeDefault()
            {
                return new Layer
                {
                    name = "Empty",
                    clip = null,
                    volume = AnimationCurve.Constant(0.0f, 1.0f, 1.0f),
                    pitch = AnimationCurve.Constant(0.0f, 1.0f, 1.0f),
                    pitcher = new MinMaxRange(1.0f, 1.0f),
                    spatialBlend = 0.0f,
                    doppler = 1.0f,
                    spread = 0.0f,
                    falloffDistance = new MinMaxRange(1.0f, 100.0f),
                    reverbMix = 1.0f
                };
            }

            public void EditorUpdate()
            {
                if (clip == null)
                {
                    this = MakeDefault();
                }
                else
                {
                    name = clip.name;
                }
            }

#endif

            public AudioClip clip;

            [AnimationCurveLimit(0.0f, 1.0f, 0.0f, 1.0f)]
            public AnimationCurve volume;

            [AnimationCurveLimit(0.0f, 1.0f, 0.5f, 2.0f)]
            public AnimationCurve pitch;

            public MinMaxRange pitcher;

            [Range(0.0f, 1.0f)]
            public float spatialBlend;

            [Range(0.0f, 5.0f)]
            public float doppler;

            [Range(0.0f, 360.0f)]
            public float spread;

            public MinMaxRange falloffDistance;

            [Range(0.0f, 1.1f)]
            public float reverbMix;

        }

#pragma warning restore CA2235


        public MinMaxRange valueRange = new MinMaxRange(0.0f, 1.0f);

        public Layer[] layers;

        protected float GetLayerValue(float _value) => valueRange.GetClampedProgress(_value);

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < layers?.Length; i++)
            {
                layers[i].EditorUpdate();
            }
        }
#endif

        protected void SetValue(float _value)
        {
            if (m_playingLayers != null)
            {
                float normalizedValue = valueRange.GetClampedProgress(_value);
                foreach (PlayingLayer layer in m_playingLayers)
                {
                    layer.SetValue(normalizedValue);
                }
            }
        }

        protected void StartPlaying(bool _loop)
        {
            if (m_playingLayers != null)
            {
                foreach (PlayingLayer layer in m_playingLayers)
                {
                    layer.Play(_loop);
                }
            }
        }

        private void OnDisable()
        {
            foreach (PlayingLayer layer in m_playingLayers)
            {
                layer.Stop();
            }
        }

        private class PlayingLayer
        {

            private readonly AudioSource m_audioSource;
            private readonly Layer m_layer;
            private float m_pitchModifier;
            private float m_value;

            public PlayingLayer(GameObject _parent, Layer _layer)
            {
                m_audioSource = _parent.AddComponent<AudioSource>();
                m_audioSource.clip = _layer.clip;
                m_layer = _layer;
                m_audioSource.reverbZoneMix = _layer.reverbMix;
                m_audioSource.spatialBlend = _layer.spatialBlend;
                m_audioSource.spread = _layer.spread;
                m_audioSource.dopplerLevel = _layer.doppler;
                m_audioSource.maxDistance = _layer.falloffDistance.max;
                m_audioSource.minDistance = _layer.falloffDistance.min;
            }

            private void UpdateSource()
            {
                m_audioSource.pitch = m_layer.pitch.Evaluate(m_value) * m_pitchModifier;
                m_audioSource.volume = m_layer.volume.Evaluate(m_value);
            }

            public void Play(bool _loop)
            {
                m_pitchModifier = m_layer.pitcher.Random;
                UpdateSource();
                m_audioSource.loop = _loop;
                m_audioSource.Play();
            }

            public void SetValue(float _value)
            {
                m_value = _value;
                UpdateSource();
            }

            public void Stop() => m_audioSource.Stop();

            public void Destroy()
            {
                if (m_audioSource != null)
                {
                    Object.Destroy(m_audioSource);
                }
            }

        }

        private PlayingLayer[] m_playingLayers;

        private void Start()
        {
            m_playingLayers = new PlayingLayer[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                m_playingLayers[i]?.Destroy();
                m_playingLayers[i] = new PlayingLayer(gameObject, layers[i]);
            }
            AudioReady();
        }

        protected virtual void AudioReady()
        {

        }

    }

}
