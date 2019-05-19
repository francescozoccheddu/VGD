using UnityEngine;
using Wheeled.Scene;

namespace Wheeled.Gameplay.Offense
{
    public sealed class RocketBehaviour : MonoBehaviour
    {
        public ParticleSystem particleSystemTrail;
        public MeshRenderer meshRenderer;

        public GameObject explosion;

        private Color m_color = Color.red;

        public void Dissolve()
        {
            particleSystemTrail.Stop();
            if (Application.isPlaying)
            meshRenderer.enabled = false;
        }

        private void OnEnable()
        {
            meshRenderer.enabled = true;
            
        }

        public void Explode()
        {
            Explode(transform.position);
        }

        public void Explode(Vector3 _position)
        {
            transform.position = _position;
            if (Application.isPlaying)
            {
                var gameObject = Instantiate(explosion, _position, Quaternion.identity);
                ParticlesColorUtils.SetChildrenRendererColor(gameObject, m_color);
            }
            particleSystemTrail.Stop();
            meshRenderer.enabled = false;
        }

        public void Move(Vector3 _position)
        {
            transform.LookAt(_position);
            transform.position = _position;
        }

        public void SetColor(Color _color)
        {
            m_color = _color;
            ParticlesColorUtils.SetChildrenRendererColor(gameObject, _color);
        }

    }
}