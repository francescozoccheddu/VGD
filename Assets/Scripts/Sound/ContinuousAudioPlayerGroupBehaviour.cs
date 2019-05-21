using UnityEngine;

namespace Wheeled.Sound
{

    public sealed class ContinuousAudioPlayerGroupBehaviour
    {

        public float value;

        public ContinuousAudioPlayer[] audioPlayers;

        public void ReachValue()
        {
            foreach (var audioPlayer in audioPlayers)
            {
                audioPlayer.ReachValue();
            }
        }

        private void Update()
        {
            foreach (var audioPlayer in audioPlayers)
            {
                audioPlayer.value = value;
            }
        }

    }

}
