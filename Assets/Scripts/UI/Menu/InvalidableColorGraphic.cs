using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class InvalidableColorGraphic : MonoBehaviour
    {

        public Graphic validColor;
        public Color invalidColor;
        public Graphic target;
        private bool m_isValid;

        public bool IsValid
        {
            get => m_isValid;
            set
            {
                target.color = value ? validColor.color : invalidColor;
                m_isValid = value;
            }
        }

    }
}
