using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public sealed class CrossHairBehaviour : MonoBehaviour
    {
        #region Public Fields

        public GameObject crossHairGroup;
        public CanvasGroup hitMarkerGroup;
        public Text healthText;

        #endregion Public Fields

        #region Private Fields

        private const float c_hitMarkerDuration = 2.0f;
        private static CrossHairBehaviour s_instance;

        #endregion Private Fields

        #region Internal Methods

        internal static void SetHealth(int _health)
        {
            string newText = _health.ToString();
            if (s_instance.healthText.text != newText)
            {
                s_instance.healthText.text = newText;
            }
        }

        internal static void NotifyHit()
        {
            s_instance.hitMarkerGroup.alpha = 1.0f;
        }

        internal static void SetEnabled(bool _enabled)
        {
            s_instance.crossHairGroup.SetActive(_enabled);
        }

        #endregion Internal Methods

        #region Private Methods

        private void Update()
        {
            s_instance.hitMarkerGroup.alpha -= Time.deltaTime * c_hitMarkerDuration;
        }

        private void Start()
        {
            s_instance = this;
            SetEnabled(false);
        }

        #endregion Private Methods
    }
}