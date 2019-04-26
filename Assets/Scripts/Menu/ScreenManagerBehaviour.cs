using UnityEngine;

namespace Wheeled.Menu
{
    public sealed class ScreenManagerBehaviour : MonoBehaviour
    {
        #region Public Fields

        public GameObject menuScreen;

        #endregion Public Fields

        #region Private Fields

        private GameObject m_lastScreen;

        #endregion Private Fields

        #region Public Methods

        public void Navigate(GameObject _screen)
        {
            m_lastScreen?.SetActive(false);
            _screen?.SetActive(true);
            m_lastScreen = _screen;
        }

        #endregion Public Methods

        #region Private Methods

        private void Awake()
        {
            Navigate(menuScreen);
        }

        #endregion Private Methods
    }
}