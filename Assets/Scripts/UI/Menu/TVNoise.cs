using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.Menu
{
    public sealed class TVNoise : MonoBehaviour
    {

        public RawImage noiseImage;
        public Light light;

        [Range(0.0f, 1.0f)]
        public float alpha = 0.5f;

        [Range(0.01f, 0.5f)]
        public float probability = 0.1f;

        [Range(0.1f, 10.0f)]
        public float frequency = 1.0f;

        [Range(0.0f, 10.0f)]
        public float lightIntensity = 0.5f;

        [Range(0.0f, 10.0f)]
        public float noiseLightIntensity = 0.5f;

        private float m_time;
        private float m_seed;

        private void Awake()
        {
            m_seed = Random.Range(0.0f, 100.0f);
        }

        private void Update()
        {
            m_time += Time.deltaTime * frequency;
            float value = Mathf.PerlinNoise(m_time, m_seed);
            float noise = 1.0f - Mathf.Clamp01(value / probability);
            Color color = noiseImage.color;
            color.a = noise * alpha;
            noiseImage.color = color;
            light.intensity = Mathf.Lerp(lightIntensity, noiseLightIntensity, noise);
        }

    }
}
