using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class ColorPreferenceTabBehaviour : MonoBehaviour
    {
        #region Public Properties

        public int ColorIndex
        {
            get => m_colorIndex;
            set
            {
                m_colorIndex = value;
                graphic.color = Scripts.PlayerPreferences.colors[value];
            }
        }

        #endregion Public Properties

        #region Public Fields

        public Graphic graphic;

        #endregion Public Fields

        #region Private Fields

        private int m_colorIndex;

        #endregion Private Fields
    }
}