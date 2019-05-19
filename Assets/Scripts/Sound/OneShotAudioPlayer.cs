using UnityEditor;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Sound
{

    public sealed class OneShotAudioPlayer : AudioPlayer
    {

        public void Play(float _value)
        {
            SetValue(_value);
            StartPlaying(false);
        }

    }

}
