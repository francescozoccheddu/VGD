using UnityEngine;

namespace Wheeled.Gameplay.Stage
{
    public sealed class RocketProjectileBehaviour : MonoBehaviour
    {
        internal void Dissolve()
        {
            Destroy(gameObject);
        }

        internal void Explode(Vector3 _position)
        {
            transform.position = _position;
            Destroy(gameObject);
        }

        internal void Move(Vector3 _position)
        {
            transform.position = _position;
        }

        internal void Shoot(Vector3 _origin, Vector3 _direction)
        {
            transform.position = _origin;
        }
    }
}