using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Core.Utils
{

    public sealed class CoInstantiatorBehaviour : MonoBehaviour
    {

        public GameObject prefab;

        private void Awake()
        {
            Instantiate(prefab, transform.position, transform.rotation);
        }

    }

}
