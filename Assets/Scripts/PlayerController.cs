using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private const bool c_pedanticPrevision = true;
    private const int c_simulationFrequency = 30;
    private const int c_networkFrequency = 10;
    private const float c_clientHistoryLength = 5.0f;
    private const float c_serverHistoryLength = 5.0f;

    private struct TransformState
    {
        public Vector3 position;
        public float lookUp;
        public float turn;

        public void Apply(Transform transform)
        {
            transform.position = position;
            transform.eulerAngles = new Vector3(lookUp, turn);
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public void Capture(Transform transform)
        {
            position = transform.position;
            lookUp = transform.eulerAngles.x;
            turn = transform.eulerAngles.y;
        }

    }

    private struct SimulationState
    {
        public TransformState transform;
        public Vector3 velocity;
    }

    private struct InputState
    {
        public Vector2 movement;
        public bool jump;
        public bool dash;
    }

    private struct PlayerState
    {
        public SimulationState simulation;
        public double deltaTime;
    }


    public CharacterController characterController;
    public float speed;
    public float jumpForce;

    public const float movementForce = 5.0f;
    public const float dashImpulse = 1.0f;
    public const float jumpImpulse = 10.0f;
    public const float dragForce = 1.0f;
    public const float dashStaminaGrowth = 1.0f;
    public const bool relativeDashImpulse = false;
    public const float gravityForce = 10.0f;

    private float m_dashStamina;
    private Vector3 m_velocity;

    private static float SetToZero(float value, float speed, float deltaTime)
    {
        return Mathf.Max(0.0f, Mathf.Abs(value) - speed * deltaTime) * Mathf.Sign(value);
    }

    private void Simulate(float deltaTime, InputState input)
    {
        m_dashStamina = Mathf.Min(1.0f, m_dashStamina + dashStaminaGrowth * deltaTime);
        if (characterController.isGrounded)
        {
            Vector2 relMovement = Vector2.ClampMagnitude(input.movement, 1.0f) * movementForce * deltaTime;
            if (input.dash)
            {
                if (relativeDashImpulse)
                {
                    float multiplier = 1.0f + m_dashStamina * dashImpulse;
                    m_dashStamina -= relMovement.magnitude;
                    relMovement *= multiplier;
                }
                else
                {
                    Vector2 direction = relMovement.normalized;
                    relMovement += direction * m_dashStamina * dashImpulse;
                    m_dashStamina = 0.0f;
                }
            }
            if (input.jump)
            {
                m_velocity.y = jumpImpulse;
            }
            m_velocity += transform.TransformDirection(relMovement);
        }
        m_velocity.x = SetToZero(m_velocity.x, dragForce, deltaTime);
        m_velocity.y -= gravityForce * deltaTime;
        m_velocity.z = SetToZero(m_velocity.z, dragForce, deltaTime);
        characterController.Move(m_velocity * deltaTime);
    }

    private void Update()
    {
        bool inputJump = Input.GetButtonDown("Jump");
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Simulate(Time.deltaTime, new InputState { jump = inputJump, movement = new Vector2(inputX, inputY) });

        float rotY = Input.GetAxis("Mouse X");
        gameObject.transform.eulerAngles = new Vector3(0.0f, gameObject.transform.eulerAngles.y + rotY, 0.0f);
    }
}
