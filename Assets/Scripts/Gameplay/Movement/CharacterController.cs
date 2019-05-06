using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Movement;

internal struct CharacterController
{
    #region Public Properties

    public Vector3 Position { get => m_capsule.position; set => m_capsule.position = value; }
    public Vector3 Velocity { get => m_capsule.velocity; set => m_capsule.velocity = value; }
    public float Height { get => m_capsule.height; set => m_capsule.height = value; }
    public bool IsGrounded => m_capsule.IsGrounded;

    #endregion Public Properties

    #region Public Fields

    public float dashStamina;

    #endregion Public Fields

    #region Private Fields

    private const float c_jumpSpeed = 5.0f;
    private const float c_dashSpeed = 20.0f;
    private const float c_moveAcceleration = 20.0f;
    private const float c_maxAccelerationSpeed = 6.0f;
    private const float c_gravityY = -10.0f;
    private const float c_dashRegenSpeed = 0.5f;
    private const float c_airMovementFactor = 0.5f;
    private const float c_maxSpeedXZ = 10.0f;
    private const float c_groundDragXZ = 5.0f;
    private const float c_airDragXZ = 1.0f;

    private CapsuleController m_capsule;

    #endregion Private Fields

    #region Public Methods

    public static bool AreNearlyEqual(in CharacterController _a, in CharacterController _b)
    {
        return CapsuleController.AreNearlyEqual(_a.m_capsule, _b.m_capsule);
    }

    public static CharacterController Lerp(in CharacterController _a, in CharacterController _b, float _progress)
    {
        return new CharacterController
        {
            m_capsule = CapsuleController.Lerp(_a.m_capsule, _b.m_capsule, _progress),
            dashStamina = Mathf.Lerp(_a.dashStamina, _b.dashStamina, _progress)
        };
    }

    public CharacterController Simulate(InputStep _input, float _deltaTime)
    {
        return Simulate(new Vector2(_input.movementX, _input.movementZ), _input.jump, _input.dash, _deltaTime);
    }

    public CharacterController Simulate(Vector2 _movementXZ, bool _jump, bool _dash, float _deltaTime)
    {
        CharacterController next = this;
        next.dashStamina = Mathf.Clamp01(next.dashStamina + _deltaTime * c_dashRegenSpeed);
        _movementXZ = Vector2.ClampMagnitude(_movementXZ, 1.0f);
        if (next.m_capsule.IsGrounded)
        {
            if (_jump)
            {
                next.m_capsule.velocity.y = c_jumpSpeed;
            }
            if (_dash)
            {
                float stamina = next.dashStamina;
                next.dashStamina = Mathf.Max(next.dashStamina - _movementXZ.magnitude, 0.0f);
                next.m_capsule.velocity += (stamina * c_dashSpeed * _movementXZ).ToVector3XZ();
            }
        }
        else
        {
            _movementXZ *= c_airMovementFactor;
        }
        next.m_capsule.velocity.y += c_gravityY * _deltaTime;
        {
            Vector3 offset = Vector3.up * (CapsuleController.c_height / 2.0f - CapsuleController.c_radius);
            foreach (Collider collider in Physics.OverlapCapsule(next.Position + offset, next.Position - offset, CapsuleController.c_radius, Scripts.Collisions.jumpPad))
            {
                JumpPadBehaviour behaviour = collider.GetComponent<JumpPadBehaviour>();
                if (behaviour != null)
                {
                    float force = behaviour.GetForce(next.Position);
                    next.m_capsule.velocity.y += force * _deltaTime;
                }
            }
        }
        Vector2 velocityXZ = next.m_capsule.velocity.ToVector2XZ();
        {
            float currentSpeed = velocityXZ.magnitude;
            float compliance = Mathf.Max(Vector2.Dot(velocityXZ.normalized, _movementXZ.normalized), 0.0f);
            float clampFactor = 1.0f - Mathf.Clamp01(currentSpeed / c_maxAccelerationSpeed) * compliance;
            velocityXZ += Vector2.ClampMagnitude(_movementXZ, clampFactor) * _deltaTime * c_moveAcceleration;
        }
        {
            float targetSpeed = velocityXZ.magnitude;
            if (targetSpeed > 0.0f)
            {
                float drag = next.m_capsule.IsGrounded ? c_groundDragXZ : c_airDragXZ;
                float targetMagnitude = Mathf.Max(0.0f, Mathf.Min(targetSpeed - drag * _deltaTime, c_maxSpeedXZ));
                velocityXZ = velocityXZ * (targetMagnitude / targetSpeed);
                next.m_capsule.velocity = velocityXZ.ToVector3XZ(next.m_capsule.velocity.y);
            }
        }
        next.m_capsule = next.m_capsule.Simulate(_deltaTime);
        return next;
    }

    #endregion Public Methods
}