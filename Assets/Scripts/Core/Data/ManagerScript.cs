using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class ManagerScript : ScriptableObject
    {
        public CollisionScript collisions;

        public ActorScript actors;

        public PlayerPreferencesScript playerPreferences;

        public SceneScript scenes;
    }
}