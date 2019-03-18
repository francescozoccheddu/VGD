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
        public bool isAlive;

        private Vector3 m_position;
        private Sight m_sight;

        public PlayerView()
        {
            isPositionInterpolationEnabled = false;
            isSightInterpolationEnabled = false;
            positionInterpolationQuickness = 4.0f;
            sightInterpolationQuickness = 2.0f;
        }

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

        public void Spawn(Vector3 _spawnPoint)
        {
            Debug.DrawRay(_spawnPoint + Vector3.forward, Vector3.forward * -2, Color.magenta, 2f);
            Debug.DrawRay(_spawnPoint + Vector3.right,  Vector3.right * -2, Color.magenta, 2f);
        }

        public void Die(Vector3 _position)
        {
            Object.Instantiate(ScriptManager.Actors.corpse, _position, Quaternion.identity);
        }

        public void ShootRocket(Vector3 _from, Vector3 _shootDirection)
        {
            Debug.DrawRay(_from, _shootDirection, Color.blue, 2f);
        }

        public void ShootRifle(Vector3 _from, Vector3 _shootDirection, float _power)
        {

            Debug.DrawRay(_from, _shootDirection, Color.Lerp(Color.green, Color.red, _power), 2f);
        }


        public void Update(float _deltaTime)
        {
            EnsureSpawned();

            // Life
            m_gameObject.SetActive(isAlive);
            
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

            if (Input.GetKeyDown(KeyCode.P))
            {
                Spawn(m_position);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                Die(m_position);
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                isAlive = !isAlive;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ShootRocket(m_position, Quaternion.Euler(0,m_sight.Turn, 0) * Vector3.forward * 4);
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                ShootRifle(m_position, Quaternion.Euler(0,m_sight.Turn, 0) * Vector3.forward * 4, 0.5f);
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
