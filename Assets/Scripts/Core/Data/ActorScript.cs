using UnityEngine;
using Wheeled.Gameplay.PlayerView;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class ActorScript : ScriptableObject
    {
        [Header("Player")]
        public GameObject collisionProbe;
        public GameObject player;

        [Header("Offense")]
        public GameObject rifleProjectile;
        public GameObject rocketProjectile;
        public GameObject explosion;
    }
}