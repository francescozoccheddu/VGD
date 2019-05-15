using UnityEngine;

namespace Wheeled.Gameplay.Offense
{
    public sealed class RocketProjectileBehaviour : MonoBehaviour
    {
        public ParticleSystem particleSystemTrail;
        public MeshRenderer meshRenderer;

        public GameObject explosion;

        public void Dissolve()
        {
            particleSystemTrail.Stop();
            meshRenderer.enabled = false;
        }

        public void Explode(Vector3 _position)
        {
            transform.position = _position;

            Instantiate(explosion, _position, Quaternion.identity);
            particleSystemTrail.Stop();
            meshRenderer.enabled = false;
        }

        public void Move(Vector3 _position)
        {
            transform.LookAt(_position);
            transform.position = _position;
        }

        public void Shoot(Vector3 _origin, Vector3 _direction)
        {
            transform.position = _origin;
        }
    }
}