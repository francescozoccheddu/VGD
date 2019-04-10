using UnityEngine;

namespace Wheeled.Gameplay.Stage
{
    public sealed class RifleProjectileBehaviour : MonoBehaviour
    {
        public ParticleSystem particleSystem;

        private const float c_lifeTime = 1.0f;

        private float m_elapsedTime;

        internal void Shoot(Vector3 _origin, Vector3 _end, bool _hit)
        {
            m_elapsedTime = 0.0f;
            transform.position = _end;
            Vector3 scale = new Vector3(1.0f, 1.0f, (_end - _origin).magnitude);
            transform.localScale = scale;
            transform.localRotation = Quaternion.LookRotation(_origin - _end);
            if (_hit)
            {
                particleSystem.Play();
            }
        }

        private void Update()
        {
            m_elapsedTime += Time.deltaTime;
            if (m_elapsedTime >= c_lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}