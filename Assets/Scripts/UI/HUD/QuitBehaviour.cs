using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.Assets.Scripts.HUD
{
    public sealed class QuitBehaviour : MonoBehaviour
    {
        public Text text;
        public Animator animator;

        private const int c_quitTime = 2;
        private float m_elapsedTime;

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
                animator.SetBool("IsVisible", true);
            }
            else
            {
                m_elapsedTime = 0.0f;
                text.text = "Hold <b>ESC</b> to quit";
                animator.SetBool("IsVisible", false);
            }
        }
    }
}