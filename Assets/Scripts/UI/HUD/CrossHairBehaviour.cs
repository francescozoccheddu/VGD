using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.HUD
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {

        public Graphic left;
        public Graphic right;

        public bool IsLeftEnabled
        {
            get => m_isLeftEnabled;
            set
            {
                left.color = value ? enabledColor : disabledColor;
                m_isLeftEnabled = value;
            }
        }
        public bool IsRightEnabled
        {
            get => m_isRightEnabled;
            set
            {
                right.color = value ? enabledColor : disabledColor;
                m_isRightEnabled = value;
            }
        }

        public Color enabledColor;
        public Color disabledColor;
        private bool m_isLeftEnabled;
        private bool m_isRightEnabled;
    }
}