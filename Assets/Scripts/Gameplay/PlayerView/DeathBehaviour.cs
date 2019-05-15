﻿using UnityEngine;

namespace Wheeled.Gameplay.PlayerView
{
    public sealed class DeathBehaviour : MonoBehaviour
    {
        #region Public Properties

        public CameraBehaviour cameraBehaviour;
        public RifleDisplayBehaviour rifleDisplayBehaviour;

        public bool IsDead { get; private set; }

        #endregion Public Properties

        #region Internal Methods

        public Animator animator;

        internal void Die(Vector3 _velocity)
        {
            if (!IsDead)
            {
                animator.SetBool("Is Died", true);
                IsDead = true;
                cameraBehaviour.SetLocal(false);
                rifleDisplayBehaviour.text.enabled = false;
                rifleDisplayBehaviour.enabled = false;
                GetComponent<DamperBehaviour>().enabled = false;
                GetComponent<SightBehaviour>().enabled = false;
                // TODO Disable behaviourss
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    c.enabled = true;
                }
                foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
                {
                    rb.detectCollisions = true;
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.isKinematic = false;
                    rb.velocity = _velocity;
                }
            }
        }


        #endregion Internal Methods
    }
}