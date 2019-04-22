using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {
        #region Public Fields

        public RawImage image;
        public Text text;

        #endregion Public Fields

        #region Private Fields

        private static CrossHairBehaviour s_instance;

        #endregion Private Fields

        #region Internal Methods

        internal static void SetHealth(int _health)
        {
            string newText = _health.ToString();
            if (s_instance.text.text != newText)
            {
                s_instance.text.text = newText;
            }
        }

        internal static void SetEnabled(bool _enabled)
        {
            s_instance.image.enabled = _enabled;
            s_instance.text.enabled = _enabled;
        }

        #endregion Internal Methods

        #region Private Methods

        private void Start()
        {
            s_instance = this;
            SetEnabled(false);
        }

        #endregion Private Methods
    }
}