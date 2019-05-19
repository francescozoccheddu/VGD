using UnityEditor;
using UnityEngine;

namespace Wheeled.Sound
{
    public sealed class AudioPlayer : MonoBehaviour
    {

        public float value;

        public bool useDelta;

        public AnimationCurve modifier;

        public AudioLayer[] layers;

    }

}
