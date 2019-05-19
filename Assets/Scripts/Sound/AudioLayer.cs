using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Wheeled.Core.Utils;

namespace Wheeled.Sound
{
    [System.Serializable]
    public sealed class AudioLayer
    {

        public AnimationCurve volume = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        public AnimationCurve pitch = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        [MinMaxRangeLimit(0.5f, 2.0f)]
        public MinMaxRange randomPitch = new MinMaxRange(1.0f, 1.0f);

    }

}
