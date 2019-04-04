using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class ActorScript : ScriptableObject
    {
        public GameObject corpse;
        public GameObject player;

        public GameObject rifleProjectile;
        public GameObject rocketProjectile;
    }
}