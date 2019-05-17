using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Scene
{
    public sealed class CameraShakeBehaviour : MonoBehaviour
    {

        public int seed;

        public Vector3 movement = Vector3.one;

        public Vector3 rotation = Vector3.one;

        [Range(0.05f, 10.0f)]
        public float frequency = 1.0f;

        [Range(0.0f, 2.0f)]
        public float intensity = 1.0f;

        private float m_time;

        private float Get(int _comp)
        {
            return (Mathf.PerlinNoise(m_time, _comp + seed * 6) * 2.0f - 1.0f) * intensity;
        }

        private void OnEnable()
        {
            m_time = 0.0f;
        }
        private void Update()
        {
            m_time += Time.deltaTime * frequency;
            transform.localPosition = Vector3.Scale(new Vector3(Get(0), Get(1), Get(2)), movement);
            transform.localEulerAngles = Vector3.Scale(new Vector3(Get(3), Get(4), Get(5)), rotation);
        }


    }

}
