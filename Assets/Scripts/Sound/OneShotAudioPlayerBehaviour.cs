using UnityEditor;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Sound
{

    public sealed class OneShotAudioPlayerBehaviour : AudioPlayerBehaviour
    {

        public bool playOnAwake;

        private void Awake()
        {
            if (playOnAwake)
            {
                Play(1.0f);
            }
        }

        public void Play(float _value)
        {
            SetValue(_value);
            StartPlaying(false);
        }

    }

}
