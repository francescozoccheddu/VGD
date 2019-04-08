using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed class SpawnManagerBehaviour : MonoBehaviour
    {

        public static SpawnManagerBehaviour Instance { get; private set; }

        public SpawnManagerBehaviour()
        {
            Instance = this;
        }

        public Transform[] points;

    }
}
