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
    public sealed class TextControlAsset : PlayableAsset
    {

        public ExposedReference<Text> element;
        [TextArea(3,4)]
        public string text;
        [Range(8, 24)]
        public int size = 12;

        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner)
        {
            var playable = ScriptPlayable<TextControlBehaviour>.Create(_graph);
            var behaviour = playable.GetBehaviour();
            behaviour.text = text;
            behaviour.size = size;
            behaviour.element = element.Resolve(_graph.GetResolver());
            return playable;
        }

    }
}
