using UnityEngine;
using UnityEngine.Playables;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.PlayerView;

namespace Wheeled.Scene
{
    public sealed class MenuIntroBehaviour : MonoBehaviour
    {

        private const string c_firstTimeKey = "firstTime";

        private static bool s_shown = false;

        public PlayableDirector intro;
        public PlayableDirector introShort;

        public MaterialBehaviour killerMaterial;
        public RocketBehaviour rocketMaterial;
        public MaterialBehaviour scrapMaterial;
        public HeadBehaviour killerHead;


        private static T GetRandom<T>(T[] _array)
        {
            return _array[Random.Range(0, _array.Length - 1)];
        }

        private void RandomizeScene()
        {
            Color killerColor = GetRandom(Scripts.PlayerPreferences.colors);
            Debug.Log(killerColor);
            killerMaterial.Color = killerColor;
            rocketMaterial.SetColor(killerColor);
            scrapMaterial.Color = GetRandom(Scripts.PlayerPreferences.colors);
            killerHead.SetHead(GetRandom(Scripts.PlayerPreferences.heads).prefab);
        }

        private void Awake()
        {
            RandomizeScene();
            if (!s_shown)
            {
                bool firstTime = PlayerPrefs.GetInt(c_firstTimeKey, 1) != 0;
                if (firstTime)
                {
                    PlayerPrefs.SetInt(c_firstTimeKey, 0);
                    PlayerPrefs.Save();
                    intro.Play();
                }
                else
                {
                    introShort.Play();
                }
                s_shown = true;
            }
        }

    }

}
