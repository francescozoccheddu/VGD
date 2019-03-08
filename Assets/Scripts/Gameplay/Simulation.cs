using UnityEngine;

namespace Wheeled.Gameplay
{

    internal struct SimulationStep
    {

        private const float c_jumpSpeed = 10;
        private const float c_gravitySpeed = -5;

        public Vector3 velocity;
        public Vector3 position;

        public SimulationStep Simulate(in InputStep _input, float _deltaTime)
        {
            SimulationStep next = this;
            next.velocity.x = _input.movementX;
            next.velocity.z = _input.movementZ;
            if (next.position.y == 2.0f)
            {
                if (_input.jump)
                {
                    next.velocity.y = c_jumpSpeed;
                }
                else
                {
                    next.velocity.y = 0.0f;
                }
            }
            next.velocity.y += c_gravitySpeed * _deltaTime;
            next.position += velocity * _deltaTime;
            if (next.position.y <= 2.0f)
            {
                next.position.y = 2.0f;
            }
            return next;
        }

        public static SimulationStep Lerp(in SimulationStep _a, in SimulationStep _b, float _progress)
        {
            SimulationStep l;
            l.velocity = Vector3.Lerp(_a.velocity, _b.velocity, _progress);
            l.position = Vector3.Lerp(_a.position, _b.position, _progress);
            return l;
        }

        private static bool IsNearlyEqual(float _a, float _b)
        {
            return Mathf.Approximately(_a, _b);
        }

        private static bool IsNearlyEqual(in Vector3 _a, in Vector3 _b)
        {
            return IsNearlyEqual(_a.x, _b.x)
                && IsNearlyEqual(_a.y, _b.y)
                && IsNearlyEqual(_a.z, _b.z);
        }

        public static bool IsNearlyEqual(in SimulationStep _a, in SimulationStep _b)
        {
            return IsNearlyEqual(_a.velocity, _b.velocity)
                && IsNearlyEqual(_a.position, _b.position);
        }

    }

    internal struct Snapshot
    {

        public SimulationStep simulation;
        public float turn;
        public float lookUp;

        public static Snapshot Lerp(in Snapshot _a, in Snapshot _b, float _progress)
        {
            Snapshot l;
            l.simulation = SimulationStep.Lerp(_a.simulation, _b.simulation, _progress);
            l.turn = Mathf.Lerp(_a.turn, _b.turn, _progress);
            l.lookUp = Mathf.Lerp(_a.lookUp, _b.lookUp, _progress);
            return l;
        }

    }

    internal struct InputStep
    {

        public float movementX;
        public float movementZ;
        public bool jump;
        public bool dash;

        public InputStep Predicted
        {
            get
            {
                InputStep predicted = this;
                predicted.jump = false;
                predicted.dash = false;
                return predicted;
            }
        }

        private static bool IsNearlyEqual(float _a, float _b)
        {
            return Mathf.Approximately(_a, _b);
        }

        public static bool IsNearlyEqual(in InputStep _a, in InputStep _b)
        {
            return IsNearlyEqual(_a.movementX, _b.movementX)
                && IsNearlyEqual(_a.movementZ, _b.movementZ)
                && _a.dash == _b.dash
                && _a.jump == _b.jump;
        }


    }

}
