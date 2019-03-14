using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay
{

    internal sealed class PlayerView
    {

        private GameObject m_gameObject;

        public bool isPositionInterpolationEnabled;
        public bool isSightInterpolationEnabled;
        public float positionInterpolationQuickness;
        public float sightInterpolationQuickness;

        private Vector3 m_position;
        private Sight m_sight;

        private void EnsureSpawned()
        {
            if (m_gameObject == null)
            {
                m_gameObject = Object.Instantiate(ScriptManager.Actors.player);
            }
        }

        public void Move(in Snapshot _snapshot)
        {
            m_position = _snapshot.simulation.position;
            m_sight = _snapshot.sight;
        }

        public void Spawn()
        {

        }

        public void Die()
        {

        }

        public void Update(float _deltaTime)
        {
            EnsureSpawned();
            // Position
            if (isPositionInterpolationEnabled)
            {
                float lerpAlpha = Mathf.Min(1.0f, _deltaTime * positionInterpolationQuickness);
                m_gameObject.transform.position = Vector3.LerpUnclamped(m_gameObject.transform.position, m_position, lerpAlpha);
            }
            else
            {
                m_gameObject.transform.position = m_position;
            }
            // Sight
            if (isSightInterpolationEnabled)
            {
                float lerpAlpha = Mathf.Min(1.0f, _deltaTime * sightInterpolationQuickness);
                Vector3 angles = m_gameObject.transform.eulerAngles;
                angles.y = Mathf.LerpAngle(angles.y, m_sight.Turn, lerpAlpha);
                m_gameObject.transform.eulerAngles = angles;
            }
            else
            {
                Vector3 angles = m_gameObject.transform.eulerAngles;
                angles.y = m_sight.Turn;
                m_gameObject.transform.eulerAngles = angles;
            }
        }

        public void Destroy()
        {
            if (m_gameObject != null)
            {
                Object.Destroy(m_gameObject);
            }
            m_gameObject = null;
        }
    }

}
