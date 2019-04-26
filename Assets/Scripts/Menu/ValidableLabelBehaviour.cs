using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class ValidableLabelBehaviour : MonoBehaviour
    {
        #region Public Fields

        [Header("Color")]
        public Color validColor = Color.white;
        public Color invalidColor = Color.red;

        [Header("Graphics")]
        public Graphic graphic;

        #endregion Public Fields

        #region Public Methods

        public void SetValid(bool _valid)
        {
            graphic.color = _valid ? validColor : invalidColor;
        }

        #endregion Public Methods
    }
}