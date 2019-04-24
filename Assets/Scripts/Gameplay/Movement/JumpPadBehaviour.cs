using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    public class JumpPadBehaviour : MonoBehaviour
    {
        #region Public Fields

        [Header("Inner radius")]
        public float innerRadiusXZ;
        public float innerRadiusY;

        [Header("Outer radius")]
        public float outerRadiusXZ;
        public float outerRadiusY;

        [Header("Force")]
        public float force = 50.0f;

        #endregion Public Fields

        #region Internal Methods

        internal float GetForce(Vector3 _position)
        {
            float factorXZ, factorY;
            {
                float distanceXZ = Vector2.Distance(_position.ToVector2XZ(), transform.position.ToVector2XZ());
                factorXZ = 1.0f - Mathf.Clamp01((distanceXZ - innerRadiusXZ) / (outerRadiusXZ - innerRadiusXZ));
            }
            {
                float diffY = _position.y - transform.position.y;
                if (diffY < 0.0f)
                {
                    factorY = 0.0f;
                }
                else
                {
                    factorY = 1.0f - Mathf.Clamp01((diffY - innerRadiusY) / (outerRadiusY - innerRadiusY));
                }
            }
            float factor = factorXZ * factorY;
            return force * factor;
        }

        #endregion Internal Methods
    }
}