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

        public static TransformState Capture(PlayerController playerController)
        {
            return new TransformState
            {
                position = playerController.transform.position,
                lookUp = playerController.transform.eulerAngles.x,
                turn = playerController.transform.eulerAngles.y
            };
        }

        public static TransformState Lerp(TransformState a, TransformState b, float progress)
        {
            return new TransformState
            {
                position = Vector3.Lerp(a.position, b.position, progress),
                lookUp = Mathf.Lerp(a.lookUp, b.lookUp, progress),
                turn = Mathf.Lerp(a.turn, b.turn, progress),
            };
        }

        public void Apply(PlayerController playerController)
        {
            playerController.transform.position = position;
            playerController.transform.eulerAngles = new Vector3(lookUp, turn);
            playerController.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

    }

    private struct SimulationState
    {
        public TransformState transform;
        public Vector3 velocity;
        public float dashStamina;

        public void Apply(PlayerController playerController)
        {
            transform.Apply(playerController);
            playerController.m_velocity = velocity;
            playerController.m_dashStamina = dashStamina;
        }

        public static SimulationState Capture(PlayerController playerController)
        {
            return new SimulationState
            {
                transform = TransformState.Capture(playerController),
                velocity = playerController.m_velocity,
                dashStamina = playerController.m_dashStamina
            };
        }

        public static SimulationState Lerp(SimulationState a, SimulationState b, float progress)
        {
            return new SimulationState
            {
                transform = TransformState.Lerp(a.transform, b.transform, progress),
                velocity = Vector3.Lerp(a.velocity, b.velocity, progress),
                dashStamina = Mathf.Lerp(a.dashStamina, b.dashStamina, progress)
            };
        }

    }

    private struct InputState
    {
        public Vector3 movementXZ;
        public bool jump;
        public bool dash;
        public float turn;
    }

    private struct PlayerState
    {
        public SimulationState simulation;
        public double deltaTime;
    }

    public CharacterController characterController;
    public float speed;
    public float jumpForce;

    public const float movementForce = 100.0f;
    public const float dashImpulse = 1.0f;
    public const float jumpImpulse = 5.0f;
    public const float groundDragForce = 50.0f;
    public const float airDragForce = 5.0f;
    public const float dashStaminaGrowth = 1.0f;
    public const bool relativeDashImpulse = false;
    public const float gravityForce = 10.0f;
    public const float maxSpeed = 10.0f;

    private float m_dashStamina;
    private Vector3 m_velocity;

    private static float UpdateSpeed(float speed, float drag, float max, float deltaTime)
    {
        if (speed > 0)
        {
            speed = speed - drag * deltaTime;
            return speed < 0.0f ? 0.0f : speed > max ? max : speed;
        }
        else
        {
            speed = speed + drag * deltaTime;
            return speed > 0.0f ? 0.0f : speed < -max ? -max : speed;
        }
    }

    public static Vector2 RotateMovementInputXZ(float right, float forward, float turn)
    {
        float angleRad = Mathf.Deg2Rad * turn;
        float sin = Mathf.Sin(angleRad);
        float cos = Mathf.Cos(angleRad);
        return new Vector2
        {

            x = (cos * right) + (sin * forward),
            y = (cos * forward) - (sin * right)
        };
    }

    private void Simulate(float deltaTime, InputState input)
    {
        m_dashStamina = Mathf.Min(1.0f, m_dashStamina + dashStaminaGrowth * deltaTime);
        float dragForce = airDragForce;
        if (characterController.isGrounded)
        {
            Vector2 relMovement = Vector2.ClampMagnitude(input.movementXZ, 1.0f) * movementForce * deltaTime;
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
            m_velocity.x += input.movementXZ.x;
            m_velocity.z += input.movementXZ.y;
            dragForce += groundDragForce;
        }
        m_velocity.x = UpdateSpeed(m_velocity.x, dragForce, maxSpeed, deltaTime);
        m_velocity.y -= gravityForce * deltaTime;
        m_velocity.z = UpdateSpeed(m_velocity.z, dragForce, maxSpeed, deltaTime);
        characterController.Move(m_velocity * deltaTime);
    }

    private void SimulateFixedTimestep(float deltaTime, float timestep, bool allowSmallerTimesteps, InputState input)
    {
        while (deltaTime > timestep)
        {
            Simulate(timestep, input);
            deltaTime -= timestep;
        }
        if (!Mathf.Approximately(deltaTime, 0.0f))
        {
            if (!Mathf.Approximately(deltaTime, timestep))
            {
                if (allowSmallerTimesteps)
                {
                    Simulate(deltaTime, input);
                }
                else
                {
                    SimulationState last = SimulationState.Capture(this);
                    Simulate(timestep, input);
                    SimulationState.Lerp(last, SimulationState.Capture(this), deltaTime / timestep).Apply(this);
                }
            }
            else
            {
                Simulate(timestep, input);
            }
        }
    }

    public const float minOutgoingDeltaTime = 1 / 90.0f;

    private InputState m_outgoingInput;
    private float m_outgoingDeltaTime;
    private float m_outgoingTimestamp;

    private void SendState(InputState input, SimulationState simulation, float deltaTime)
    {
        m_outgoingInput.dash |= input.dash;
        m_outgoingInput.jump |= input.jump;
        m_outgoingInput.movementXZ += input.movementXZ;
        m_outgoingDeltaTime += deltaTime;
        if (m_outgoingDeltaTime > minOutgoingDeltaTime)
        {
            // Push input (clamp), timestamp, simulation, deltaTime
            m_outgoingTimestamp += m_outgoingDeltaTime;
            m_outgoingDeltaTime = 0.0f;
            m_outgoingInput.dash = false;
            m_outgoingInput.jump = false;
            m_outgoingInput.movementXZ = Vector3.zero;
        }
    }

    public const float maxDeltaTime = 1 / 10.0f;

    private void Update()
    {
        float deltaTime = Mathf.Min(maxDeltaTime, Time.deltaTime);

        bool inputJump = Input.GetButtonDown("Jump");
        bool inputDash = false; //Input.GetButtonDown("Dash");
        float inputMovX = Input.GetAxis("Horizontal");
        float inputMovY = Input.GetAxis("Vertical");
        float inputLookX = Input.GetAxis("Mouse X");
        float inputLookY = Input.GetAxis("Mouse Y");

        Vector3 actAngles = transform.eulerAngles;
        actAngles.x = 0;
        actAngles.y += inputLookX;
        actAngles.z = 0;

        gameObject.transform.eulerAngles = actAngles;

        InputState input;
        input.turn = actAngles.y;
        input.movementXZ = RotateMovementInputXZ(inputMovX, inputMovY, input.turn);
        input.jump = inputJump;
        input.dash = inputDash;

        SimulateFixedTimestep(Time.deltaTime, 1 / 60f, true, input);

        SendState(input, SimulationState.Capture(this), deltaTime);
    }
}
