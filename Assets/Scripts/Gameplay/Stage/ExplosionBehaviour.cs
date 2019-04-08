using UnityEngine;

namespace Wheeled.Gameplay.Stage
{
    public sealed class ExplosionBehaviour : MonoBehaviour
    {
        private float m_elapsedTime;

        internal void Explode(Vector3 _origin)
        {
            m_elapsedTime = 0.0f;
            transform.position = _origin;
        }

        private void Update()
        {
            m_elapsedTime += Time.deltaTime;
            if (m_elapsedTime > 1.0f)
            {
                Destroy(gameObject);
            }
        }
    }
}