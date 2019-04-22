using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD.CrossHair
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {
        #region Public Fields

        public RawImage image;

        #endregion Public Fields

        #region Private Fields

        private static CrossHairBehaviour s_instance;

        #endregion Private Fields

        #region Internal Methods

        internal static void Set(bool _enabled)
        {
            s_instance.image.enabled = _enabled;
        }

        #endregion Internal Methods

        #region Private Methods

        private void Start()
        {
            s_instance = this;
            Set(false);
        }

        #endregion Private Methods
    }
}