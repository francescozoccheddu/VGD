using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Utils
{
    public sealed class FishBehaviour : MonoBehaviour
    {

        public int seed = 0;

        [Range(0.01f, 3.0f)]
        public float frequency = 1.0f;

        [Range(0.0f, 5.0f)]
        public float minSpeed = 0.8f;
        [Range(0.0f, 5.0f)]
        public float maxSpeed = 1.2f;

        public Animator[] animators;

        private float m_progress;

        private void Update()
        {
            m_progress += Time.deltaTime * frequency;
            float speed = Mathf.LerpUnclamped(minSpeed, maxSpeed, Mathf.PerlinNoise(m_progress, seed));
            foreach (Animator animator in animators)
            {
                animator.speed = speed;
            }
        }

    }
}
