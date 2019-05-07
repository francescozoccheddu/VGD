﻿using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    public class JumpPadBehaviour : MonoBehaviour
    {
        

        [Header("Inner radius")]
        public float innerRadiusXZ;
        public float innerRadiusY;

        [Header("Outer radius")]
        public float outerRadiusXZ;
        public float outerRadiusY;

        [Header("Force")]
        public float force = 50.0f;

        public const float c_averageViewMass = 3.0f;

        internal float GetForce(Vector3 _position)
        {
            float factorXZ, factorY;
            {
                float distanceXZ = Vector2.Distance(_position.ToVector2XZ(), transform.position.ToVector2XZ());
                factorXZ = 1.0f - Mathf.Clamp01((distanceXZ - innerRadiusXZ) / (outerRadiusXZ - innerRadiusXZ));
            }
            {
                float diffY = _position.y - transform.position.y;
                if (diffY < 0.0f)
                {
                    factorY = 0.0f;
                }
                else
                {
                    factorY = 1.0f - Mathf.Clamp01((diffY - innerRadiusY) / (outerRadiusY - innerRadiusY));
                }
            }
            float factor = factorXZ * factorY;
            return force * factor;
        }

        private readonly HashSet<Rigidbody> m_rigidbodies = new HashSet<Rigidbody>();

        private void OnTriggerEnter(Collider _other)
        {
            m_rigidbodies.Add(_other.attachedRigidbody);
        }

        private void OnTriggerExit(Collider _other)
        {
            m_rigidbodies.Remove(_other.attachedRigidbody);
        }

        private void Update()
        {
            m_rigidbodies.RemoveWhere(_rb => _rb == null);
            foreach (var rb in m_rigidbodies)
            {
                rb.AddForce(0.0f, force * c_averageViewMass, 0.0f, ForceMode.Force);
            }
        }

        
    }
}