using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wheeled.Menu
{
    public sealed class HeadFanBehaviour : MonoBehaviour
    {

        [Range(1.0f, 50.0f)]
        public float acceleration = 10.0f;

        private float m_speed;
        private float m_target;

        public void SetSpeed(float _speed)
        {
            m_speed = _speed;
            m_target = 0.0f;
        }

        public void Accelerate(float _targetSpeed)
        {
            m_target = _targetSpeed;
        }

        private void Update()
        {
            if (m_speed > m_target)
            {
                m_speed -= acceleration * Time.deltaTime;
                if (m_speed < m_target)
                {
                    m_speed = m_target;
                }
            }
            else
            {
                m_speed += acceleration * Time.deltaTime;
                if (m_speed > m_target)
                {
                    m_speed = m_target;
                }
            }
            m_target = 0.0f;
            transform.Rotate(Vector3.up, m_speed * Time.deltaTime);
        }

    }
}
