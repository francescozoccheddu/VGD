using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.HUD
{
    public sealed class HealthIndicatorBehaviour : MonoBehaviour
    {
        [Header("Component")]
        public Text text;

        [Header("Color")]
        public Color color;
        public Color criticalColor;
        public Color damageColor;
        [Range(0.2f, 5.0f)]
        public float colorInterpolationQuickness;

        [Header("Health")]
        public int criticalHealth;
        [Range(5f, 20.0f)]
        public float textInterpolationQuickness;

        public int Health { get; set; }

        private float m_health;

        public void NotifyDamage() => text.color = damageColor;
        private void Update()
        {
            m_health = Mathf.Lerp(m_health, Health, Time.deltaTime * textInterpolationQuickness);
            text.text = Mathf.RoundToInt(m_health).ToString();
            Color target = m_health <= criticalHealth ? criticalColor : color;
            text.color = Color.Lerp(text.color, target, Time.deltaTime * colorInterpolationQuickness);
        }

        private void OnEnable()
        {
            m_health = 0.0f;
            text.color = criticalColor;
        }

    }
}
