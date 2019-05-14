using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.HUD.DamageMarker
{
    public sealed class DamageMarkerBehaviour : MonoBehaviour
    {

        public Vector3 Position { get; private set; }

        private Camera m_lastCamera;

        public Shadow shadow;

        [Range(1.0f, 5.0f)]
        public float shadowDistance = 1.0f;
        [Range(1.0f, 50.0f)]
        public float interpolationQuickness = 1.0f;

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void Set(Vector3 _position)
        {
            Position = _position;
            transform.eulerAngles = new Vector3(0.0f, 0.0f, CalculateRotation());
        }

        private float CalculateRotation()
        {
            Camera camera = DamageMarkerManagerBehaviour.Instance.Camera;
            if (camera == null)
            {
                if (m_lastCamera == null)
                {
                    return 0.0f;
                }
                camera = m_lastCamera;
            }
            else
            {
                m_lastCamera = camera;
            }
            Vector3 look = camera.transform.forward;
            Vector3 direction = (Position - camera.transform.position).normalized;
            float dot = Vector3.Dot(look, direction);
            if (Mathf.Approximately(dot, 0.0f) || Mathf.Approximately(dot, 1.0f))
            {
                return 90.0f;
            }
            else if (dot == -1.0f)
            {
                return -90.0f;
            }
            else
            {
                return Vector3.SignedAngle(direction, camera.transform.right, Vector3.up);
            }
        }

        private void Update()
        {
            float target = CalculateRotation();
            float rotation = Mathf.LerpAngle(transform.eulerAngles.z, target, Time.deltaTime * interpolationQuickness);
            transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
            float shadowAngle = Mathf.Deg2Rad * (-rotation - 45.0f);
            Vector2 shadowOffset = new Vector2(Mathf.Cos(shadowAngle), Mathf.Sin(shadowAngle));
            shadow.effectDistance = shadowOffset * shadowDistance;
        }

    }
}
