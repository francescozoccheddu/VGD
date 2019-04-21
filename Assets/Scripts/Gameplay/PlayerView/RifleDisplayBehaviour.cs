using UnityEngine;
using UnityEngine.UI;

public class RifleDisplayBehaviour : MonoBehaviour
{
    #region Internal Properties

    internal float Power { get; set; }

    #endregion Internal Properties

    #region Public Fields

    public Text text;

    public Color baseColor;
    public Color fixColor;

    #endregion Public Fields

    #region Private Fields

    private const float c_fixDuration = 1.0f;
    private float m_timeSinceLastShot;
    private float m_fixPower;

    private string m_cachedText;

    #endregion Private Fields

    #region Internal Methods

    internal void Shoot(float _power)
    {
        m_timeSinceLastShot = 0.0f;
        m_fixPower = _power;
    }

    #endregion Internal Methods

    #region Private Methods

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
        text.color = fix ? fixColor : baseColor;
    }

    #endregion Private Methods
}