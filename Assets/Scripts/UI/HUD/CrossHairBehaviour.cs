using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.HUD
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {

        public Graphic graphic;
        public Animator animator;

        private bool m_enabled;

        public bool IsEnabled
        {
            get => m_enabled; set
            {
                m_enabled = value;
                if (animator.isActiveAndEnabled)
                {
                    animator.SetBool("IsEnabled", value);
                }
            }
        }

        private void Awake()
        {
            IsEnabled = m_enabled;
        }

        public Color Color { get => graphic.color; set => graphic.color = value; }

    }
}