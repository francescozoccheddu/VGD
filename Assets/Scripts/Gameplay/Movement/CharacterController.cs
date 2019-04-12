using UnityEngine;

public struct CharacterController
{

    private const float c_jumpSpeed = 10.0f;
    private const float c_dashSpeed = 20.0f;
    private const float c_moveAcceleration = 20.0f;
    private const float c_maxAccelerationSpeed = 6.0f;
    private const float c_gravityY = -20.0f;
    private const float c_dashRegenSpeed = 0.5f;
    private const float c_airMovementFactor = 0.5f;
    private const float c_maxSpeedXZ = 10.0f;
    private const float c_groundDragXZ = 5.0f;
    private const float c_airDragXZ = 1.0f;

    public CapsuleController capsule;
    public float dashStamina;

    public static bool AreNearlyEqual(in CharacterController _a, in CharacterController _b)
    {
        return CapsuleController.AreNearlyEqual(_a.capsule, _b.capsule);
    }

    public CharacterController Simulate(Vector2 _movementXZ, bool _jump, bool _dash, float _deltaTime)
    {
        CharacterController next = this;
        next.dashStamina = Mathf.Clamp01(next.dashStamina + _deltaTime * c_dashRegenSpeed);
        _movementXZ = Vector2.ClampMagnitude(_movementXZ, 1.0f);
        if (next.capsule.IsGrounded)
        {
            if (_jump)
            {
                next.capsule.velocity.y = c_jumpSpeed;
            }
            if (_dash)
            {
                float stamina = next.dashStamina;
                next.dashStamina = Mathf.Max(next.dashStamina - _movementXZ.magnitude, 0.0f);
                next.capsule.velocity += (stamina * c_dashSpeed * _movementXZ).ToVector3XZ();
            }
        }
        else
        {
            _movementXZ *= c_airMovementFactor;
        }
        next.capsule.velocity.y += c_gravityY * _deltaTime;
        Vector2 velocityXZ = next.capsule.velocity.ToVector2XZ();
        {
            float currentSpeed = velocityXZ.magnitude;
            float compliance = Mathf.Max(Vector2.Dot(velocityXZ.normalized, _movementXZ.normalized), 0.0f);
            float clampFactor = 1.0f - Mathf.Clamp01(currentSpeed / c_maxAccelerationSpeed) * compliance;
            Lebug.Log("ClampFactor", clampFactor);
            velocityXZ += Vector2.ClampMagnitude(_movementXZ, clampFactor) * _deltaTime * c_moveAcceleration;
        }
        {
            float targetSpeed = velocityXZ.magnitude;
            if (targetSpeed > 0.0f)
            {
                float drag = next.capsule.IsGrounded ? c_groundDragXZ : c_airDragXZ;
                float targetMagnitude = Mathf.Max(0.0f, Mathf.Min(targetSpeed - drag * _deltaTime, c_maxSpeedXZ));
                velocityXZ = velocityXZ * (targetMagnitude / targetSpeed);
                next.capsule.velocity = velocityXZ.ToVector3XZ(next.capsule.velocity.y);
            }
            Lebug.Log("SpeedXZ", next.capsule.velocity.ToVector2XZ().magnitude);
            Lebug.Log("SpeedY", next.capsule.velocity.y);
        }
        next.capsule = next.capsule.Simulate(_deltaTime);
        return next;
    }

}
