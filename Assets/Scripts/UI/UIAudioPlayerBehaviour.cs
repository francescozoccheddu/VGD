using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.UI
{
    [DisallowMultipleComponent]
    public sealed class UIAudioPlayerBehaviour : MonoBehaviour
    {

        public UIAudioSourceBehaviour Source => GetComponentInParent<UIAudioSourceBehaviour>();

        public void Play(AudioClip _clip)
        {
            Source.Play(_clip);
        }

    }
}
