using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Utils;
using Wheeled.Sound;

namespace Wheeled.UI.Menu
{
    public sealed class ScreenNoiseBehaviour : NoiseBehaviour
    {

        public RawImage noiseImage;
        public new Light light;

        [Range(0.0f, 1.0f)]
        public float alpha = 0.5f;

        public ContinuousAudioPlayerBehaviour sound;

        public MinMaxRange lightIntensity = new MinMaxRange(1.0f, 3.0f);

        protected override void NoiseUpdated(float _noise) { 
            sound.value = _noise;
            Color color = noiseImage.color;
            color.a = _noise * alpha;
            noiseImage.color = color;
            light.intensity = lightIntensity.LerpUnclamped(1.0f - _noise);
        }

    }
}
