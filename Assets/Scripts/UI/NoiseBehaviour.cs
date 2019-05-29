using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Utils;
using Wheeled.Sound;

namespace Wheeled.UI
{
    public abstract class NoiseBehaviour : MonoBehaviour
    {

        [Range(0.01f, 0.5f)]
        public float probability = 0.1f;

        [Range(0.1f, 10.0f)]
        public float frequency = 1.0f;

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
            NoiseUpdated(noise);
        }

        protected abstract void NoiseUpdated(float _noise);

    }
}
