using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD.CrossHair
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {
        #region Public Fields

        public RawImage rocketImage;
        public RawImage rifleImage;
        public CanvasGroup group;

        #endregion Public Fields

        #region Private Fields

        private static CrossHairBehaviour s_instance;

        #endregion Private Fields

        #region Internal Methods

        internal static void SetBase(bool _active)
        {
            s_instance.group.alpha = _active ? 1.0f : 0.0f;
        }

        internal static void Set(bool _rifle, bool _rocket)
        {
            s_instance.rocketImage.enabled = _rifle;
            s_instance.rifleImage.enabled = _rocket;
        }

        #endregion Internal Methods

        #region Private Methods

        private void Start()
        {
            s_instance = this;
            SetBase(false);
            Set(true, true);
        }

        #endregion Private Methods
    }
}