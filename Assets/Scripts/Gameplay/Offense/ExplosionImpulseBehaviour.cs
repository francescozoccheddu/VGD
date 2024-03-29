﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Gameplay.Offense
{
    public class ExplosionImpulseBehaviour : MonoBehaviour
    {

        public LayerMask layerMask;
        [Range(0.1f, 10.0f)]
        public float radius;
        [Range(8.0f, 80.0f)]
        public float force;

        protected void Apply(float _intensity)
        {
            float netForce = force * _intensity;
            if (netForce > 0.0f && radius > 0.0f)
            {
                foreach (Collider collider in Physics.OverlapSphere(transform.position, radius, layerMask))
                {
                    collider.attachedRigidbody.AddExplosionForce(netForce, transform.position, radius, 3.0f, ForceMode.Impulse);
                }
            }
        }

        private void Start()
        {
            Apply(1.0f);
        }

    }
}
