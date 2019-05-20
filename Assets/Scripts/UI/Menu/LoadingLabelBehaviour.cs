using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.Menu
{
    public sealed class LoadingLabelBehaviour : MonoBehaviour
    {
        public Text text;
        public string message;

        private const int c_totalDotCount = 3;
        private const float c_period = 1 / 2.0f;

        private int m_dotCount;

        private void OnEnable()
        {
            InvokeRepeating(nameof(UpdateText), c_period, c_period);
            m_dotCount = 0;
            UpdateText();
        }

        private void UpdateText()
        {
            text.text = message + new string('.', m_dotCount);
            m_dotCount = (m_dotCount + 1) % (c_totalDotCount + 1);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }
    }
}