using UnityEngine;

public class MaterialBehaviour : MonoBehaviour
{

    public Material opaque;
    public Material transparent;
    private float m_alpha = 1.0f;
    private Color m_color = Color.red;

    public float Alpha
    {
        get => m_alpha;
        set
        {
            m_alpha = value;
            if (m_material != null && m_alpha < 1.0f && !m_isTransparent)
            {
                DestroyMaterial();
                m_isTransparent = true;
                m_material = Instantiate(transparent);
                SetMaterial();
            }
            SetAlpha();
        }
    }

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

    private void SetAlpha() => m_material.SetFloat("_Alpha", m_alpha);

    private void Start()
    {
        m_isTransparent = false;
        m_material = Instantiate(opaque);
        SetColor();
        SetMaterial();
    }

    private void OnDestroy() => DestroyMaterial();

    private Material m_material;
    private bool m_isTransparent;

}