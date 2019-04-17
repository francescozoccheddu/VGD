using UnityEngine;

namespace Wheeled.Gameplay.Stage
{
    public sealed class RifleProjectileBehaviour : MonoBehaviour
    {
        public GameObject rifleHit;
        public ParticleSystem particles;
        
        private const float c_lifeTime = 1.0f;
        private const float c_maxRayLenght = 40f;
        private const int c_maxParticles = 100;

        private float m_elapsedTime;

        internal void Shoot(Vector3 _origin, Vector3 _end, bool _hit)
        {
            m_elapsedTime = 0.0f;
            transform.position = _origin;
            Vector3 ray = _end - _origin;
            Vector3 scale = new Vector3(1.0f, 1.0f, ray.magnitude);
            transform.localScale = scale;
            transform.localRotation = Quaternion.LookRotation(ray);

            float rayDensity = Mathf.Clamp01(ray.magnitude / c_maxRayLenght);
            int particleCount = Mathf.RoundToInt(rayDensity * c_maxParticles);

            particles.emission.SetBurst(0, new ParticleSystem.Burst { count = particleCount });
            

            if (_hit)
            {
                Instantiate(rifleHit, _end, transform.rotation);
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