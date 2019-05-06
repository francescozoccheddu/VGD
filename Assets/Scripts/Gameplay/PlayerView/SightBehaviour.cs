using UnityEngine;

public class SightBehaviour : MonoBehaviour
{

    [Header("Transforms")]
    public Transform head;
    public Transform arm;
    public Transform torso;
    public Transform cameraArm;

    [Header("LookUp")]
    public float lookUpSpeed = 5.0f;
    public float maxLookUpOffset = 30.0f;
    public float headLookUpFactor = 0.3f;
    private float m_lookUp;

    [Header("Turn")]
    public float turnSpeed = 5.0f;
    public float maxTurnOffset = 30.0f;
    private float m_turn;

    internal float LookUp { get; set; }
    public float Turn { get; set; }

    internal void ReachTarget()
    {
        m_turn = Turn;
        m_lookUp = LookUp;
    }

    private float Lerp(float _current, float _target, float _speed, float _maxOffset)
    {
        float angle = Mathf.LerpAngle(_current, _target, _speed * Time.deltaTime);
        float diff = Mathf.DeltaAngle(angle, _target);
        if (Mathf.Abs(diff) > _maxOffset)
        {
            angle += diff - Mathf.Sign(diff) * _maxOffset;
        }
        return angle;
    }

    private void Update()
    {
        m_turn = Lerp(m_turn, Turn, turnSpeed, maxTurnOffset);
        m_lookUp = Lerp(m_lookUp, LookUp, lookUpSpeed, maxLookUpOffset);

        head.localRotation = Quaternion.Euler(m_lookUp * headLookUpFactor, 0.0f, 0.0f);
        arm.localRotation = Quaternion.Euler(m_lookUp, 0.0f, 0.0f);
        torso.localRotation = Quaternion.Euler(0.0f, m_turn, 0.0f);
        cameraArm.localRotation = Quaternion.Euler(LookUp, Turn, 0.0f);
    }
}
