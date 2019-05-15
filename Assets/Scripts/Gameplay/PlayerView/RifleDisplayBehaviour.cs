using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Gameplay.PlayerView
{
    public class RifleDisplayBehaviour : MonoBehaviour
    {
        public float Power { get; set; }

        public Text text;

        public Color BaseColor { get; set; } = Color.red;
        public Color FixColor { get; set; } = Color.gray;

        private const float c_fixDuration = 1.0f;
        private float m_timeSinceLastShot;
        private float m_fixPower;

        private string m_cachedText;

        public void Shoot(float _power)
        {
            m_timeSinceLastShot = 0.0f;
            m_fixPower = _power;
        }

        private void Start()
        {
            m_timeSinceLastShot = c_fixDuration;
        }

        private void Update()
        {
            m_timeSinceLastShot += Time.deltaTime;
            bool fix = m_timeSinceLastShot < c_fixDuration;
            float power = fix ? m_fixPower : Power;
            string newText = Mathf.RoundToInt(power * 100.0f).ToString();
            if (newText != text.text)
            {
                text.text = newText;
            }
            text.color = fix ? FixColor : BaseColor;
        }
    }
}