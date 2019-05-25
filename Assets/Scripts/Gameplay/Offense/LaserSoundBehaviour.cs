using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Gameplay.Offense
{
    public sealed class LaserSoundBehaviour : MonoBehaviour
    {

        public float velocity;

        public float Distance { get; set; }

        private Vector3 m_origin;

        private void Start()
        {
            m_origin = transform.position;
        }

        private void Update()
        {
            transform.position += transform.forward * velocity * Time.deltaTime;
            if (Vector3.Distance(transform.position, m_origin) >= Distance)
            {
                Destroy(gameObject);
            }
        }

    }
}
