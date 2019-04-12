using UnityEngine;

public static class CapsuleControllerHelper
{

    public static Vector3 ToVector3XZ(this Vector2 _vector, float _y = 0.0f)
    {
        return new Vector3(_vector.x, _y, _vector.y);
    }

    public static Vector2 ToVector2XZ(this Vector3 _vector)
    {
        return new Vector2(_vector.x, _vector.z);
    }

    public static bool AreNearlyEqual(float _a, float _b, float _maxOffset)
    {
        return Mathf.Abs(_a - _b) <= _maxOffset;
    }

    public static bool AreNearlyEqualPerComponent(in Vector3 _a, in Vector3 _b, float _maxOffset)
    {
        return AreNearlyEqual(_a.x, _b.x, _maxOffset)
            && AreNearlyEqual(_a.y, _b.y, _maxOffset)
            && AreNearlyEqual(_a.z, _b.z, _maxOffset);
    }

    public static bool AreNearlyEqualPerDistance(in Vector3 _a, in Vector3 _b, float _maxOffset)
    {
        return Vector3.SqrMagnitude(_a - _b) <= _maxOffset * _maxOffset;
    }

    public static bool AreNearlyEqualPerOffset(in Vector3 _a, in Vector3 _b, float _maxMagnitudeOffset, float _maxAngleOffset)
    {
        return Vector3.Angle(_a, _b) <= _maxAngleOffset
            && AreNearlyEqual(_a.magnitude, _b.magnitude, _maxMagnitudeOffset);
    }

}

public struct CapsuleController
{
    public Vector3 position;
    public Vector3 velocity;
    public bool IsGrounded => height <= c_skin + c_groundOffset;
    public float height;

    private const int c_layerMask = 1 << 0;
    private const float c_gameCeilingY = 5.0f;
    private const float c_gameFloorY = -5.0f;
    private const float c_height = 2.0f;
    private const int c_maxMoveIteractions = 5;
    private const float c_radius = 0.5f;
    private const float c_overShoot = 0.5f;
    private const float c_skin = 0.1f;
    private const float c_groundOffset = 0.01f;
    private static readonly Vector3 s_emergencyRespawnPoint = new Vector3(0.0f, 5.0f, 0.0f);
    private const float c_maxSlopeAngle = 50.0f;
    private const float c_maxHeight = 10.0f;

    public static bool AreNearlyEqual(in CapsuleController _a, in CapsuleController _b)
    {
        return CapsuleControllerHelper.AreNearlyEqualPerDistance(_a.position, _b.position, 0.01f)
            && CapsuleControllerHelper.AreNearlyEqualPerOffset(_a.velocity, _b.velocity, 0.2f, 0.5f);
    }

    public CapsuleController Simulate(float _deltaTime)
    {
        CapsuleController next = this;
        next.EmergencyDepenetrationY();
        next.EarlyDepenetrationY();
        next.MoveY(_deltaTime);
        next.MoveXZ(_deltaTime);
        next.CalculateHeight();
        Lebug.Log("Height", next.height);
        return next;
    }

    private void EmergencyDepenetrationY()
    {
        if (position.y <= c_gameFloorY)
        {
            position.y = c_gameCeilingY;
            const float capsulePointOffset = c_height / 2.0f - c_radius;
            RaycastHit[] hits = Physics.RaycastAll(position, -Vector3.up);
            for (int i = hits.Length - 1; i >= 0; i--)
            {
                position.y = hits[i].point.y + c_height / 2.0f + c_skin;
                if (!Physics.CheckCapsule(position - Vector3.up * capsulePointOffset, position + Vector3.up * capsulePointOffset, c_radius))
                {
                    Debug.LogWarning("Emergency floor reset");
                    velocity = Vector3.zero;
                    return;
                }
            }
            position = s_emergencyRespawnPoint;
            velocity = Vector3.zero;
            Debug.LogWarning("Emergency respawn");
        }
    }

    private void MoveXZ(float _deltaTime)
    {
        const float capsulePointOffset = c_height / 2.0f - c_radius;
        float initialDistance = velocity.magnitude * _deltaTime;
        float initialDeltaTime = _deltaTime;
        int iterations = 0;
        while (iterations++ < c_maxMoveIteractions && velocity != Vector3.zero && _deltaTime > 0.0f)
        {
            float movementDistance = velocity.magnitude * _deltaTime;
            Vector3 direction = velocity.normalized;
            Vector3 pointA = position - Vector3.up * capsulePointOffset;
            Vector3 pointB = position + Vector3.up * capsulePointOffset;
            if (Physics.CapsuleCast(pointA, pointB, c_radius, direction, out RaycastHit hit, movementDistance + c_overShoot + c_skin, c_layerMask))
            {
                float distance = Mathf.Min(movementDistance, Mathf.Max(hit.distance - c_skin, 0.0f));
                position += direction * distance;

                if (distance < movementDistance)
                {
                    velocity = Slide(velocity, hit.normal);
                    DebugExtension.DebugArrow(hit.point, velocity.normalized * 2.0f);
                }

                _deltaTime *= 1.0f - Mathf.Clamp01(distance / movementDistance);
            }
            else
            {
                position += velocity * _deltaTime;
                _deltaTime = 0.0f;
            }
        }
        Lebug.Log("MovementIterations", iterations - 1);
    }

    private Vector3 Slide(in Vector3 _velocity, in Vector3 _normal)
    {
        Vector3 rightDirection = Vector3.Cross(_velocity, _normal);
        return Vector3.Cross(_normal, rightDirection);
    }

    private void ConvertCollisionY(in Vector3 _normal)
    {
        if (velocity.y != 0.0f && (velocity.y > 0.0f || Vector3.Angle(_normal, Vector3.up) > c_maxSlopeAngle))
        {
            float oldY = velocity.y;
            velocity.y = 0;
            velocity += Slide(Vector3.up * oldY, _normal);
            DebugExtension.DebugArrow(position, velocity.normalized * 3.0f, Color.yellow);
        }
        else
        {
            velocity.y = 0;
        }
    }

    private void CalculateHeight()
    {
        const float sphereBodyDistance = c_height - c_radius * 2.0f;
        const float shootDistance = sphereBodyDistance + c_maxHeight;
        Vector3 startingPosition = position + Vector3.up * (c_height / 2.0f - c_radius);
        if (Physics.SphereCast(startingPosition, c_radius, -Vector3.up, out RaycastHit hit, shootDistance, c_layerMask))
        {
            height = hit.distance - sphereBodyDistance;
        }
        else
        {
            height = c_maxHeight;
        }
    }

    private void EarlyDepenetrationY()
    {
        float sign = velocity.y > 0.0f ? 1.0f : -1.0f;
        Vector3 direction = Vector3.up * sign;
        Vector3 startingPosition = position - direction * (c_height / 2.0f - c_radius);
        if (Physics.CheckSphere(startingPosition, c_radius, c_layerMask))
        {
            if (Physics.Raycast(position - direction * (c_height / 2.0f), direction, out RaycastHit earlyHit, c_height, c_layerMask))
            {
                position.y += earlyHit.point.y - (c_height / 2.0f) * sign;
            }
        }
    }

    private void MoveY(float _deltaTime)
    {
        if (_deltaTime > 0.0f)
        {
            float amount = velocity.y * _deltaTime;
            float amountDistance = Mathf.Abs(velocity.y) * _deltaTime;

            float sign = amount > 0.0f ? 1.0f : -1.0f;
            Vector3 direction = Vector3.up * sign;

            const float sphereBodyDistance = c_height - c_radius * 2.0f;

            Vector3 startingPosition = position - direction * (c_height / 2.0f - c_radius);

            bool isFalling = sign < 0.0f;
            float moveShootDistance = amountDistance + c_overShoot + c_skin;
            float shootDistance = sphereBodyDistance + moveShootDistance;
            if (Physics.SphereCast(startingPosition, c_radius, direction, out RaycastHit hit, shootDistance, c_layerMask))
            {
                float hitDistance = hit.distance - sphereBodyDistance - c_skin;
                float movement;
                if (hitDistance > amountDistance)
                {
                    movement = amountDistance;
                }
                else
                {
                    movement = hitDistance;
                    ConvertCollisionY(hit.normal);
                }
                position.y += movement * sign;
            }
            else
            {

                position.y += amount;
            }
        }
    }

}