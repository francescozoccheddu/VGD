using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Wheeled.Tutorial
{
    public sealed class TextControlBehaviour : PlayableBehaviour
    {

        public Text element;
        public string text;
        public int size;

        public override void ProcessFrame(Playable _playable, FrameData _info, object _playerData)
        {
            if (element != null)
            {
                element.text = text;
                element.fontSize = size;
            }
        }

    }
}
