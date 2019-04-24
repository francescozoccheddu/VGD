﻿using UnityEngine;

namespace Wheeled.Gameplay.Movement
{
    internal struct InputStep
    {
        #region Public Properties

        public InputStep Clamped
        {
            get
            {
                InputStep clamped = this;
                clamped.Clamp();
                return clamped;
            }
        }
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

        #endregion Public Properties

        #region Public Fields

        public bool dash;
        public bool jump;
        public float movementX;
        public float movementZ;

        #endregion Public Fields

        #region Public Methods

        public static bool IsNearlyEqual(in InputStep _a, in InputStep _b)
        {
            return IsNearlyEqual(_a.movementX, _b.movementX)
                && IsNearlyEqual(_a.movementZ, _b.movementZ)
                && _a.dash == _b.dash
                && _a.jump == _b.jump;
        }

        public void Clamp()
        {
            float length = Mathf.Sqrt(movementX * movementX + movementZ * movementZ);
            if (length > 1.0f)
            {
                movementX /= length;
                movementZ /= length;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static bool IsNearlyEqual(float _a, float _b)
        {
            return Mathf.Approximately(_a, _b);
        }

        #endregion Private Methods
    }

    internal struct Sight
    {
        #region Public Properties

        public Vector3 Direction => Quaternion * Vector3.forward;
        public float LookUp
        {
            get => m_lookUp;
            set => m_lookUp = Mathf.Clamp(value, -40.0f, 50.0f);
        }
        public Quaternion Quaternion => Quaternion.Euler(m_lookUp, m_turn, 0.0f);
        public float Turn
        {
            get => m_turn;
            set
            {
                m_turn = value % 360.0f;
                if (m_turn < 0.0f)
                {
                    m_turn += 360.0f;
                }
            }
        }

        #endregion Public Properties

        #region Private Fields

        private float m_lookUp;
        private float m_turn;

        #endregion Private Fields

        #region Public Methods

        public static Sight Lerp(in Sight _a, in Sight _b, float _progress)
        {
            Sight l;
            l.m_turn = Mathf.LerpAngle(_a.Turn, _b.Turn, _progress);
            l.m_lookUp = Mathf.LerpAngle(_a.LookUp, _b.LookUp, _progress);
            return l;
        }

        #endregion Public Methods
    }

    internal struct SimulationStepInfo
    {
        #region Public Fields

        public InputStep input;
        public CharacterController simulation;

        #endregion Public Fields
    }

    internal struct Snapshot
    {
        #region Public Fields

        public Sight sight;
        public CharacterController simulation;

        #endregion Public Fields

        #region Public Methods

        public static Snapshot Lerp(in Snapshot _a, in Snapshot _b, float _progress)
        {
            Snapshot l;
            l.simulation = CharacterController.Lerp(_a.simulation, _b.simulation, _progress);
            l.sight = Sight.Lerp(_a.sight, _b.sight, _progress);
            return l;
        }

        #endregion Public Methods
    }
}