using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Scene
{
    public sealed class SpawnManagerBehaviour : MonoBehaviour
    {
        public Transform[] points;

        private static SpawnManagerBehaviour s_instance;

        public SpawnManagerBehaviour()
        {
            s_instance = this;
        }


        public static int Spawn(double _time)
        {
            IEnumerable<Vector3> players = from p in GameManager.Current.Players
                                             where !p.IsQuit(_time)
                                             && p.LifeHistory.IsAlive(_time)
                                             select p.GetSnapshot(_time).simulation.Position;
            float maxDistance = 0;
            int point = 0;
            for (int i = 0; i < s_instance.points.Length; i++)
            {
                float pointMinDistance = float.PositiveInfinity;
                Vector3 pointPosition = s_instance.points[i].position;
                foreach (Vector3 player in players)
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