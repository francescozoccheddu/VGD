using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public abstract class MatchBoardEventBehaviour : MonoBehaviour
    {
        #region Public Fields

        public Text text;

        #endregion Public Fields

        #region Private Fields

        private string m_cachedText;

        #endregion Private Fields

        #region Public Methods

        public void Destroy()
        {
            Destroy(gameObject);
        }

        #endregion Public Methods

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
        }

        #endregion Private Methods
    }
}