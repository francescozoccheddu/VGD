using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.Assets.Scripts.HUD
{
    public sealed class QuitBehaviour : MonoBehaviour
    {
        #region Public Fields

        public Text text;
        public CanvasGroup group;

        #endregion Public Fields

        #region Private Fields

        private const float c_fadeOutDuration = 4.0f;
        private const int c_quitTime = 3;
        private float m_elapsedTime;

        #endregion Private Fields

        #region Private Methods

        private void Update()
        {
            if (Input.GetButton("Quit"))
            {
                m_elapsedTime += Time.deltaTime;
                text.text = string.Format("Leaving game in {0} seconds", c_quitTime - Mathf.FloorToInt(m_elapsedTime));
                if (m_elapsedTime >= c_quitTime)
                {
                    GameLauncher.Instance.QuitGame();
                }
                group.alpha = 1.0f;
            }
            else
            {
                m_elapsedTime = 0.0f;
                text.text = "Hold <b>ESC</b> to leave game";
                group.alpha = Mathf.Max(0.0f, group.alpha - Time.deltaTime / c_fadeOutDuration);
            }
        }

        #endregion Private Methods
    }
}