using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.HUD
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {

        public Graphic graphic;
        public Animator animator;

        public bool IsEnabled { get => animator.GetBool("IsEnabled"); set => animator.SetBool("IsEnabled", value); }

        public Color Color { get => graphic.color; set => graphic.color = value; }
        
    }
}