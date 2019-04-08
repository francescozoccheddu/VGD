using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay
{



    internal static class SpawnManager
    {

        public static byte Spawn()
        {
            return 0;
        }

        public static Snapshot Get(byte _id)
        {
            Transform point = SpawnManagerBehaviour.Instance.points[_id];
            return new Snapshot
            {
                sight = new Sight { Turn = point.eulerAngles.y },
                simulation = new SimulationStep { position = point.position }
            };
        }

    }
}
