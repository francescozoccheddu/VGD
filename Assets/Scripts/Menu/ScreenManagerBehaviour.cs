using UnityEngine;

namespace Wheeled.Menu
{
    public sealed class ScreenManagerBehaviour : MonoBehaviour
    {
        #region Public Fields

        public GameObject menuScreen;
        public GameObject errorScreen;

        #endregion Public Fields

        #region Private Fields

        private static string s_errorMessage;
        private GameObject m_lastScreen;

        #endregion Private Fields

        #region Public Methods

        public static void SetError(string _message)
        {
            s_errorMessage = _message;
        }

        public void Navigate(GameObject _screen)
        {
            m_lastScreen?.SetActive(false);
            _screen?.SetActive(true);
            m_lastScreen = _screen;
        }

        public void Quit()
        {
            Application.Quit();
        }

        #endregion Public Methods

        #region Private Methods

        private void Awake()
        {
            if (s_errorMessage != null)
            {
                Navigate(errorScreen);
                m_lastScreen.GetComponent<ErrorScreenBehaviour>().SetMessage(s_errorMessage);
                s_errorMessage = null;
            }
            else
            {
                Navigate(menuScreen);
            }
        }

        #endregion Private Methods
    }
}