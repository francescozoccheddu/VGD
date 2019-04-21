using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{

    public const float c_radius = 0.22f;

    [Header("Transforms")]
    public Transform wheel;
    public Transform hip;

    [Header("Properties")]
    public float smoothSpeed = 10.0f;

    private Vector3 m_lastPosition;
    private float m_hipTurn;

    private void Update()
    {
        Vector3 offset = transform.position - m_lastPosition;
        Vector2 offsetXZ = new Vector2(offset.x, offset.z);
        if (offsetXZ != Vector2.zero)
        {
            float targetAngle = Vector2.SignedAngle(offsetXZ, Vector2.up);
            float currentAngle = hip.localEulerAngles.y;
            float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
            {
                float altDelta = Mathf.DeltaAngle(currentAngle + 180.0f, targetAngle);
                float path = Mathf.Abs(delta) < Mathf.Abs(altDelta) ? delta : altDelta;
                m_hipTurn = currentAngle + path;
            }
            {
                float cos = Mathf.Cos(delta * Mathf.Deg2Rad);
                float distance = offsetXZ.magnitude * cos;
                const float circumference = c_radius * 2.0f * Mathf.PI;
                float angle = distance / circumference * 360.0f;
                wheel.Rotate(Vector3.right, angle);
            }
        }
        float turn = Mathf.LerpAngle(hip.localEulerAngles.y, m_hipTurn, Time.deltaTime * smoothSpeed);
        hip.localEulerAngles = new Vector3(0, turn, 0);
        m_lastPosition = transform.position;
    }
}
