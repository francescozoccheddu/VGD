using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Utils
{
    public sealed class FishBehaviour : MonoBehaviour
    {

        public int seed = 0;

        [Range(0.01f, 3.0f)]
        public float frequency = 1.0f;

        public MinMaxRange speedRange = new MinMaxRange(0.8f, 1.5f);

        public Animator[] animators;

        private float m_progress;

        private void Update()
        {
            m_progress += Time.deltaTime * frequency;
            float speed = speedRange.LerpUnclamped(Mathf.PerlinNoise(m_progress, seed));
            foreach (Animator animator in animators)
            {
                animator.speed = speed;
            }
        }

    }
}
