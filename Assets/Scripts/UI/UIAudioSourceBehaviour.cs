using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class UIAudioSourceBehaviour : MonoBehaviour
    {

        private AudioSource m_source;

        private void Start()
        {
            m_source = GetComponent<AudioSource>();
        }

        public void Play(AudioClip _clip)
        {
            m_source.PlayOneShot(_clip);
        }

    }
}
