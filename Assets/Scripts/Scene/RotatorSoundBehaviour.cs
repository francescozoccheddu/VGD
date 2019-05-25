using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using Wheeled.Sound;

namespace Wheeled.Scene
{
    public sealed class RotatorSoundBehaviour : MonoBehaviour
    {

        public bool x;
        public bool y;
        public bool z;

        private Vector3 m_lastAngles;

        public ContinuousAudioPlayerBehaviour player;

        private void Start()
        {
            m_lastAngles = transform.eulerAngles;
        }

        private void Update()
        {
            float total = 0.0f;
            Vector3 angles = transform.eulerAngles;
            if (x)
            {
                total += Mathf.Abs(m_lastAngles.x - angles.x);
            }
            if (y)
            {
                total += Mathf.Abs(m_lastAngles.y - angles.y);
            }
            if (z)
            {
                total += Mathf.Abs(m_lastAngles.z - angles.z);
            }
            player.value = total;
            m_lastAngles = transform.eulerAngles;
        }

    }
}
