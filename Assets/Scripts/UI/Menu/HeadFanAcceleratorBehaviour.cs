using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Menu
{
    public sealed class HeadFanAcceleratorBehaviour : MonoBehaviour
    {

        public HeadFanBehaviour fan;

        public float speed;

        private void Update()
        {
            fan.Accelerate(speed);
        }

    }
}
