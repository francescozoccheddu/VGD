using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public abstract class EventBehaviour : MonoBehaviour
    {
        #region Public Fields

        public const float c_opaqueDuration = 2.0f;
        public const float c_fadeOutDuration = 1.0f;

        public Text text;

        #endregion Public Fields

        #region Private Fields

        private string m_cachedText;
        private float m_elapsedTime;

        #endregion Private Fields

        #region Protected Methods

        protected abstract string GetText();

        #endregion Protected Methods

        #region Private Methods

        private void Update()
        {
            string newText = GetText();
            if (newText != m_cachedText)
            {
                m_cachedText = newText;
                text.text = m_cachedText;
            }
            m_elapsedTime += Time.deltaTime;
            float opacity = 1.0f - Mathf.Clamp01((m_elapsedTime - c_opaqueDuration) / c_fadeOutDuration);
            if (m_elapsedTime > c_fadeOutDuration + c_opaqueDuration)
            {
                Destroy(gameObject);
            }
        }

        #endregion Private Methods
    }
}