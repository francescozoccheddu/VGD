using UnityEngine;

namespace Wheeled.Gameplay.PlayerView
{
    public class MaterialBehaviour : MonoBehaviour
    {

        public Material opaque;
        public Material transparent;
        private Color m_color = Color.red;

        public float alpha = 1.0f;

        public Color Color
        {
            get => m_color;
            set
            {
                m_color = value;
                if (m_material != null)
                {
                    SetColor();
                }
            }
        }

        private void SetMaterial()
        {
            foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
            {
                r.sharedMaterial = m_material;
            }
        }

        private void DestroyMaterial()
        {
            if (m_material != null)
            {
                Destroy(m_material);
            }
            m_material = null;
        }

        private void SetColor() => m_material.SetColor("_PaintColor", m_color);

        private void SetAlpha() => m_material.SetFloat("_Alpha", alpha);

        private void Start()
        {
            m_isTransparent = false;
            m_material = Instantiate(opaque);
            SetColor();
            SetMaterial();
        }

        private void Update()
        {
            if (m_material != null)
            {
                if (alpha < 1.0f && !m_isTransparent)
                {
                    DestroyMaterial();
                    m_isTransparent = true;
                    m_material = Instantiate(transparent);
                    SetMaterial();
                    SetColor();
                }
                if (m_isTransparent)
                {
                    SetAlpha();
                }
            }
        }

        private void OnDestroy() => DestroyMaterial();

        private Material m_material;
        private bool m_isTransparent;

    }
}