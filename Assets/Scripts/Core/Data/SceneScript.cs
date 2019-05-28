using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class SceneScript : ScriptableObject
    {
        public ArenaScript[] arenas;
        public int menuSceneBuildIndex;
        public int tutorialSceneBuildIndex;
    }
}