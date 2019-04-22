using UnityEngine;

namespace Wheeled.Gameplay.Stage
{
    public sealed class RocketProjectileBehaviour : MonoBehaviour
    {
        #region Public Fields

        public ParticleSystem particleSystemTrail;
        public MeshRenderer meshRenderer;

        public GameObject explosion;

        #endregion Public Fields

        #region Internal Methods

        internal void Dissolve()
        {
            particleSystemTrail.Stop();
            meshRenderer.enabled = false;
        }

        internal void Explode(Vector3 _position)
        {
            transform.position = _position;

            Instantiate(explosion, _position, Quaternion.identity);
            particleSystemTrail.Stop();
            meshRenderer.enabled = false;
        }

        internal void Move(Vector3 _position)
        {
            transform.LookAt(_position);
            transform.position = _position;
        }

        internal void Shoot(Vector3 _origin, Vector3 _direction)
        {
            transform.position = _origin;
        }

        #endregion Internal Methods
    }
}