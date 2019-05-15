using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.PlayerView
{
    public sealed class SocketBehaviour : MonoBehaviour
    {

        public Transform arm;

        public Vector3 GetPosition(Vector3 _playerPosition, Sight _playerSight)
        {
            Vector3 pre = transform.position - arm.position;
            return (_playerSight.Quaternion * pre) + arm.position + _playerPosition;
        }

    }
}
