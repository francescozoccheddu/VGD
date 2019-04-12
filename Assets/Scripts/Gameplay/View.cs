using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay
{

    internal sealed class PlayerView
    {

        private GameObject m_body;
        private GameObject m_corpse;

        private bool m_wasAlive;
        public bool isPositionInterpolationEnabled;
        public bool isSightInterpolationEnabled;
        public float positionInterpolationQuickness;
        public float sightInterpolationQuickness;
        public bool isAlive;

        private Vector3 m_position;
        private Sight m_sight;

        public PlayerView()
        {
            m_wasAlive = false;
            isPositionInterpolationEnabled = false;
            isSightInterpolationEnabled = false;
            positionInterpolationQuickness = 4.0f;
            sightInterpolationQuickness = 2.0f;
        }

        private void EnsureSpawned()
        {
            if (m_body == null)
            {
                m_body = Object.Instantiate(ScriptManager.Actors.player, m_position, m_sight.Quaternion);
            }
        }

        public void Move(in Snapshot _snapshot)
        {
            m_position = _snapshot.simulation.Position;
            m_sight = _snapshot.sight;
        }

        public void ShootRocket()
        {
        }

        public void ShootRifle()
        {
        }

        public void Explode()
        {

        }

        public void ReachTarget()
        {
            if (m_body != null)
            {
                m_body.transform.position = m_position;
                m_body.transform.localRotation = m_sight.Quaternion;
            }
        }

        public void Update(float _deltaTime)
        {
            EnsureSpawned();

            // Life
            if (!isAlive && m_wasAlive)
            {
                if (m_corpse != null)
                {
                    Object.Destroy(m_corpse);
                }
                m_corpse = Object.Instantiate(ScriptManager.Actors.corpse, m_position, m_sight.Quaternion);
                m_wasAlive = false;
            }
            m_wasAlive |= isAlive;
            m_body.SetActive(isAlive);

            // Position
            if (isPositionInterpolationEnabled)
            {
                float lerpAlpha = Mathf.Min(1.0f, _deltaTime * positionInterpolationQuickness);
                m_body.transform.position = Vector3.LerpUnclamped(m_body.transform.position, m_position, lerpAlpha);
            }
            else
            {
                m_body.transform.position = m_position;
            }
            // Sight
            if (isSightInterpolationEnabled)
            {
                float lerpAlpha = Mathf.Min(1.0f, _deltaTime * sightInterpolationQuickness);
                Vector3 angles = m_body.transform.eulerAngles;
                angles.y = Mathf.LerpAngle(angles.y, m_sight.Turn, lerpAlpha);
                m_body.transform.eulerAngles = angles;
            }
            else
            {
                Vector3 angles = m_body.transform.eulerAngles;
                angles.y = m_sight.Turn;
                m_body.transform.eulerAngles = angles;
            }

        }

        public void Destroy()
        {
            if (m_body != null)
            {
                Object.Destroy(m_body);
            }
            m_body = null;
        }
    }

}
