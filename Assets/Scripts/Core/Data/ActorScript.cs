using UnityEngine;

namespace Wheeled.Core.Data
{
    [CreateAssetMenu]
    public sealed class ActorScript : ScriptableObject
    {
        #region Public Fields

        [Header("Player")]
        public GameObject collisionProbe;
        public GameObject corpse;
        public GameObject player;

        [Header("Offense")]
        public GameObject rifleProjectile;
        public GameObject rocketProjectile;
        public GameObject explosion;

        [Header("MatchBoard")]
        public GameObject joinEvent;
        public GameObject quitEvent;
        public GameObject killEvent;

        #endregion Public Fields
    }
}