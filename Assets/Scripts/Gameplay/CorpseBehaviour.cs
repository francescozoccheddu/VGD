using UnityEngine;

namespace Wheeled.Gameplay
{
    public class CorpseBehaviour : MonoBehaviour
    {
        private const float c_opacqueLifeTime = 5.0f;
        private const float c_fadeOutTime = 3.0f;
        private float m_timeSinceSpawn;

        private void Start()
        {
            m_timeSinceSpawn = 0;
        }

        private void Update()
        {
            float opacity;
            m_timeSinceSpawn += Time.deltaTime;
            if (m_timeSinceSpawn >= c_opacqueLifeTime + c_fadeOutTime)
            {
                Destroy(gameObject);
            }
            else if (m_timeSinceSpawn > c_opacqueLifeTime)
            {
                opacity = 1.0f - ((m_timeSinceSpawn - c_opacqueLifeTime) / c_fadeOutTime);
            }
            else
            {
                opacity = 1.0f;
            }
        }
    }
}
