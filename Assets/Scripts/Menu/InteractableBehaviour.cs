using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class InteractableBehaviour : MonoBehaviour
    {
        #region Public Fields

        public Shadow shadow;
        public new RectTransform transform;

        #endregion Public Fields

        #region Private Fields

        private const float c_disabledScale = 0.65f;

        #endregion Private Fields

        #region Public Methods

        public void SetEnabled(bool _enabled)
        {
            if (transform != null)
            {
                transform.localScale = _enabled ? new Vector3(1, 1, 1) : new Vector3(c_disabledScale, c_disabledScale, c_disabledScale);
            }
            if (shadow != null)
            {
                shadow.enabled = _enabled;
            }
        }

        #endregion Public Methods
    }
}