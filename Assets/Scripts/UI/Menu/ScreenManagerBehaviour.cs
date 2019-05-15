using UnityEngine;

namespace Wheeled.Menu
{
    public sealed class ScreenManagerBehaviour : MonoBehaviour
    {
        public GameObject menuScreen;
        public GameObject errorScreen;

        private static string s_errorMessage;
        private static ScreenManagerBehaviour s_instance;
        private GameObject m_lastScreen;

        public static void SetError(string _message)
        {
            if (s_instance == null)
            {
                s_errorMessage = _message;
            }
            else
            {
                s_instance.DisplayError(_message);
            }
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

        private void DisplayError(string _message)
        {
            Navigate(errorScreen);
            m_lastScreen.GetComponent<ErrorScreenBehaviour>().SetMessage(_message);
        }

        private void Awake()
        {
            if (s_errorMessage != null)
            {
                DisplayError(s_errorMessage);
                s_errorMessage = null;
            }
            else
            {
                Navigate(menuScreen);
            }
        }

        private void OnEnable()
        {
            s_instance = this;
        }

        private void OnDisable()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }
    }
}