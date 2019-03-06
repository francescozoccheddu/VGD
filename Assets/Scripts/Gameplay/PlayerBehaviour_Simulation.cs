using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour
    {

        public const float speed = 5.0f;
        public const float jumpForce = 10.0f;

        public const float movementForce = 100.0f;
        public const float dashImpulse = 1.0f;
        public const float jumpImpulse = 5.0f;
        public const float groundDragForce = 5.0f;
        public const float airDragForce = 1.0f;
        public const float dashStaminaGrowth = 1.0f;
        public const bool relativeDashImpulse = false;
        public const float gravityForce = 10.0f;
        public const float maxSpeed = 10.0f;

        private float m_dashStamina;
        private Vector3 m_velocity;
        private Vector3 m_position;

        private static float UpdateSpeed(float _speed, float _drag, float _max, float _deltaTime)
        {
            if (_speed > 0)
            {
                _speed = _speed - _drag * _deltaTime;
                return _speed < 0.0f ? 0.0f : _speed > _max ? _max : _speed;
            }
            else
            {
                _speed = _speed + _drag * _deltaTime;
                return _speed > 0.0f ? 0.0f : _speed < -_max ? -_max : _speed;
            }
        }

        public struct SimulationState
        {

            public Vector3 position;
            public float lookUp;
            public float turn;
            public Vector3 velocity;
            public float dashStamina;

            public void Apply(PlayerBehaviour _playerController)
            {
                _playerController.m_position = position;
                _playerController.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                _playerController.m_velocity = velocity;
                _playerController.m_dashStamina = dashStamina;
            }

            public static SimulationState Capture(PlayerBehaviour _playerController)
            {
                return new SimulationState
                {
                    position = _playerController.m_position,
                    lookUp = _playerController.transform.eulerAngles.x,
                    turn = _playerController.transform.eulerAngles.y,
                    velocity = _playerController.m_velocity,
                    dashStamina = _playerController.m_dashStamina
                };
            }

            public static SimulationState Lerp(SimulationState _a, SimulationState _b, float _progress)
            {
                return new SimulationState
                {
                    position = Vector3.Lerp(_a.position, _b.position, _progress),
                    lookUp = Mathf.Lerp(_a.lookUp, _b.lookUp, _progress),
                    turn = Mathf.Lerp(_a.turn, _b.turn, _progress),
                    velocity = Vector3.Lerp(_a.velocity, _b.velocity, _progress),
                    dashStamina = Mathf.Lerp(_a.dashStamina, _b.dashStamina, _progress)
                };
            }

            private static bool IsNearlyEqual(float _a, float _b)
            {
                return Mathf.Approximately(_a, _b);
            }

            private static bool IsNearlyEqual(Vector3 _a, Vector3 _b)
            {
                return IsNearlyEqual(_a.x, _b.x)
                    && IsNearlyEqual(_a.y, _b.y)
                    && IsNearlyEqual(_a.z, _b.z);
            }

            public bool IsNearlyEqual(SimulationState _other)
            {
                return IsNearlyEqual(dashStamina, _other.dashStamina)
                    && IsNearlyEqual(lookUp, _other.lookUp)
                    && IsNearlyEqual(turn, _other.turn)
                    && IsNearlyEqual(velocity, _other.velocity)
                    && IsNearlyEqual(position, _other.position);
            }

        }

        public struct SimulationTime
        {
            private int m_node;
            private float m_timeSinceNode;

            public int Node
            {
                get => m_node;
                set
                {
                    m_node = value;
                    Commit();
                }
            }
            public float TimeSinceNode
            {
                get => m_timeSinceNode;
                set
                {
                    m_timeSinceNode = value;
                    Commit();
                }
            }

            public SimulationTime(int _node, float _timeSinceNode) : this()
            {
                m_node = _node;
                m_timeSinceNode = _timeSinceNode;
                Commit();
            }

            private void Commit()
            {
                m_node += Mathf.FloorToInt(m_timeSinceNode / c_timestep);
                m_timeSinceNode %= c_timestep;
                if (m_timeSinceNode < 0.0f)
                {
                    m_timeSinceNode = c_timestep - m_timeSinceNode;
                }
            }

            public override bool Equals(object _obj)
            {
                if (!(_obj is SimulationTime))
                {
                    return false;
                }

                SimulationTime other = (SimulationTime) _obj;
                return this == other;
            }

            public override int GetHashCode()
            {
                int hashCode = -1322611433;
                hashCode = hashCode * -1521134295 + m_node.GetHashCode();
                hashCode = hashCode * -1521134295 + m_timeSinceNode.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(SimulationTime _a, SimulationTime _b)
            {
                return _a.m_node == _b.m_node && _a.m_timeSinceNode == _b.m_timeSinceNode;
            }

            public static bool operator !=(SimulationTime _a, SimulationTime _b)
            {
                return !(_a == _b);
            }

            public static SimulationTime operator +(SimulationTime _a, SimulationTime _b)
            {
                return new SimulationTime(
                    _a.m_node + _b.m_node,
                    _a.m_timeSinceNode + _b.m_timeSinceNode
                );
            }

            public static SimulationTime operator -(SimulationTime _a)
            {
                return new SimulationTime
                (
                    -_a.m_node,
                    -_a.m_timeSinceNode
                );
            }

            public static SimulationTime operator -(SimulationTime _a, SimulationTime _b)
            {
                return _a + (-_b);
            }

            public static SimulationTime operator +(SimulationTime _a, float _b)
            {
                return _a + new SimulationTime { m_timeSinceNode = _b };
            }

            public static SimulationTime operator -(SimulationTime _a, float _b)
            {
                return _a + (-_b);
            }

            public static SimulationTime operator +(float _a, SimulationTime _b)
            {
                return _b + _a;
            }

            public static SimulationTime operator -(float _a, SimulationTime _b)
            {
                return (-_b) + _a;
            }

        }

        private void Simulate(InputState _input, float _deltaTime)
        {
            // Simulate XZ movement
            m_velocity.x = _input.movementX * 5;
            //m_velocity.y = -1.0f;
            m_velocity.z = _input.movementZ * 5;
            m_position += m_velocity * _deltaTime;
            m_position.y = 3;
        }

        private const float c_timestep = 1 / 20.0f;

        private SimulationState m_simulation;

    }

}