using UnityEngine;
using Wheeled.Core;

namespace Wheeled.UI.Menu
{
    public sealed class TutorialScreenBehaviour : MonoBehaviour
    {

        public ScreenManagerBehaviour screenManager;

        private const string c_shouldAskKey = "askForTutorial";

        public void DontAskForTutorial()
        {
            PlayerPrefs.SetInt(c_shouldAskKey, 0);
            PlayerPrefs.Save();
        }

        private GameObject m_requestedScreen;

        public void TryNavigate(GameObject _screen)
        {
            if (PlayerPrefs.GetInt(c_shouldAskKey, 1) != 0)
            {
                DontAskForTutorial();
                screenManager.Navigate(gameObject);
                m_requestedScreen = _screen;
            }
            else
            {
                screenManager.Navigate(_screen);
            }
        }

        public void SkipTutorial()
        {
            screenManager.Navigate(m_requestedScreen);
        }

    }
}
