using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Scene
{
    public sealed class SpawnManagerBehaviour : MonoBehaviour
    {
        public Transform[] points;

        private static SpawnManagerBehaviour s_instance;

        public SpawnManagerBehaviour()
        {
            s_instance = this;
        }

        public static int Spawn(IEnumerable<Vector3> _players)
        {
            float maxDistance = 0;
            int point = 0;
            for (int i = 0; i < s_instance.points.Length; i++)
            {
                float pointMinDistance = float.PositiveInfinity;
                Vector3 pointPosition = s_instance.points[i].position;
                foreach (Vector3 player in _players)
                {
                    pointMinDistance = Mathf.Min(Vector3.Distance(pointPosition, player), pointMinDistance);
                }
                if (pointMinDistance > maxDistance)
                {
                    point = i;
                    maxDistance = pointMinDistance;
                }
            }
            return point;
        }

        public static Snapshot Get(int _id)
        {
            Transform point = s_instance.points[_id];
            return new Snapshot
            {
                sight = new Sight { Turn = point.eulerAngles.y },
                simulation = new CharacterController { Position = point.position }
            };
        }
    }
}