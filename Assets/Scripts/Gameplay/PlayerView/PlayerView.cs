﻿using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Scene;

namespace Wheeled.Gameplay.PlayerView
{
    public sealed class PlayerView
    {

        private const float c_minGroundedHeight = 0.2f;

        public ELifeState State { get; set; }

        public bool EnableRocket {
            get => m_enableRocket;
            set
            {
                if (m_gameObject != null)
                {
                    m_gameObject.GetComponent<WeaponsBehaviour>().EnableRocket = value;
                    m_gameObject.GetComponent<MaterialBehaviour>().SetMaterial();
                }
                m_enableRocket = value;
            }
        } 

        public bool EnableRifle
        {
            get => m_enableRifle;
            set
            {
                if (m_gameObject != null)
                {
                    m_gameObject.GetComponent<WeaponsBehaviour>().EnableRifle = value;
                    m_gameObject.GetComponent<MaterialBehaviour>().SetMaterial();
                }
                m_enableRifle = value;
            }
        }

        public bool IsLocal
        {
            get => m_isLocal;
            set
            {
                if (m_gameObject != null)
                {
                    m_gameObject.GetComponent<CameraBehaviour>().SetLocal(value);
                }
                m_isLocal = value;
            }
        }

        public Color Color
        {
            get => m_color;
            set
            {
                if (m_gameObject != null)
                {
                    m_gameObject.GetComponent<MaterialBehaviour>().Color = value;
                    m_rifleDisplayBehaviour.BaseColor = value;
                }
                m_color = value;
            }
        }
        public GameObject Head
        {
            get => m_head;
            set
            {
                if (m_gameObject != null)
                {
                    m_gameObject.GetComponent<HeadBehaviour>().SetHead(value);
                }
                m_head = value;
            }
        }

        public float RiflePower { get; set; }

        public bool isPositionInterpolationEnabled;
        public bool isSightInterpolationEnabled;
        public float positionInterpolationQuickness;
        public float sightInterpolationQuickness;

        private GameObject m_gameObject;
        private DeathBehaviour m_deathBehaviour;
        private WheelBehaviour m_wheelBehaviour;
        private SightBehaviour m_sightBehaviour;
        private DamperBehaviour m_damperBehaviour;
        private RifleDisplayBehaviour m_rifleDisplayBehaviour;
        private Animator m_animator;

        private ELifeState m_lastState;
        private CharacterController m_simulation;
        private Sight m_sight;
        private bool m_isLocal;
        private bool m_enableRocket;
        private bool m_enableRifle;
        private GameObject m_explosion;
        private GameObject m_head;
        private Color m_color;

        public PlayerView()
        {
            m_lastState = State = ELifeState.Unknown;
            isPositionInterpolationEnabled = false;
            isSightInterpolationEnabled = false;
            positionInterpolationQuickness = 20.0f;
            sightInterpolationQuickness = 5.0f;
            m_isLocal = false;
            m_head = Scripts.PlayerPreferences.heads[0].prefab;
            m_enableRifle = true;
            m_enableRocket = true;
        }

        public void Move(in Snapshot _snapshot)
        {
            m_simulation = _snapshot.simulation;
            m_sight = _snapshot.sight;
        }

        public void ShootRocket() => m_gameObject?.GetComponent<WeaponsBehaviour>().ShootRocket();

        public void ShootRifle()
        {
            m_rifleDisplayBehaviour.Shoot(RiflePower);
            m_gameObject?.GetComponent<WeaponsBehaviour>().ShootLaser(RiflePower);
        }
        public void ReachTarget()
        {
            if (m_gameObject != null)
            {
                ReachSightTarget();
                ReachSimulationTarget();
                m_sightBehaviour.ReachTarget();
            }
        }

        public void Update(float _deltaTime)
        {
            if (State == ELifeState.Alive)
            {
                // Spawn
                EnsureSpawned();
                if (m_deathBehaviour.IsDead)
                {
                    Destroy();
                    EnsureSpawned();
                }
                // Move
                if (isPositionInterpolationEnabled)
                {
                    float lerpAlpha = Mathf.Min(0.9f, _deltaTime * positionInterpolationQuickness);
                    m_gameObject.transform.position = Vector3.LerpUnclamped(m_gameObject.transform.position, m_simulation.Position, lerpAlpha);
                    float height = m_simulation.Height;
                    m_damperBehaviour.height = Mathf.Lerp(m_damperBehaviour.height, height, lerpAlpha);
                    m_wheelBehaviour.isGrounded = height <= c_minGroundedHeight;
                }
                else
                {
                    ReachSimulationTarget();
                }
                // Sight
                if (isSightInterpolationEnabled)
                {
                    float lerpAlpha = Mathf.Min(0.9f, _deltaTime * sightInterpolationQuickness);
                    m_sightBehaviour.turn = Mathf.Lerp(m_sightBehaviour.turn, m_sight.Turn, lerpAlpha);
                    m_sightBehaviour.lookUp = Mathf.Lerp(m_sightBehaviour.lookUp, m_sight.LookUp, lerpAlpha);
                }
                else
                {
                    ReachSightTarget();
                }
                m_rifleDisplayBehaviour.Power = RiflePower;
            }
            else if (State == ELifeState.Unknown)
            {
                Destroy();
            }
            else
            {
                m_deathBehaviour?.Die(m_simulation.Velocity);
                if (State == ELifeState.Exploded && m_lastState != ELifeState.Exploded && m_explosion == null)
                {
                    m_explosion = Object.Instantiate(Scripts.Actors.explosion, m_simulation.Position, Quaternion.identity);
                    ParticlesColorUtils.SetChildrenRendererColor(m_explosion, Color);
                }

            }
            m_lastState = State;
        }

        public void Destroy()
        {
            if (m_gameObject != null)
            {
                Object.Destroy(m_gameObject);
            }
            m_deathBehaviour = null;
            m_damperBehaviour = null;
            m_sightBehaviour = null;
            m_animator = null;
            m_rifleDisplayBehaviour = null;
            m_gameObject = null;
        }

        private void ReachSimulationTarget()
        {
            if (m_gameObject != null)
            {
                m_gameObject.transform.position = m_simulation.Position;
                float height = m_simulation.Height;
                m_damperBehaviour.height = height;
                m_wheelBehaviour.isGrounded = height <= c_minGroundedHeight;
            }
        }

        private void ReachSightTarget()
        {
            if (m_gameObject != null)
            {
                m_sightBehaviour.turn = m_sight.Turn;
                m_sightBehaviour.lookUp = m_sight.LookUp;
            }
        }
        private void EnsureSpawned()
        {
            if (m_gameObject == null)
            {
                m_gameObject = Object.Instantiate(Scripts.Actors.player, m_simulation.Position, Quaternion.identity);
                m_gameObject.GetComponent<CameraBehaviour>().SetLocal(IsLocal);
                m_wheelBehaviour = m_gameObject.GetComponent<WheelBehaviour>();
                m_damperBehaviour = m_gameObject.GetComponent<DamperBehaviour>();
                m_sightBehaviour = m_gameObject.GetComponent<SightBehaviour>();
                m_deathBehaviour = m_gameObject.GetComponent<DeathBehaviour>();
                m_gameObject.GetComponent<HeadBehaviour>().SetHead(m_head);
                m_rifleDisplayBehaviour = m_gameObject.GetComponent<RifleDisplayBehaviour>();
                m_rifleDisplayBehaviour.Power = RiflePower;
                m_rifleDisplayBehaviour.BaseColor = m_color;
                m_animator = m_gameObject.GetComponent<Animator>();
                var weaponsBehaviour = m_gameObject.GetComponent<WeaponsBehaviour>();
                weaponsBehaviour.EnableRocket = m_enableRocket;
                weaponsBehaviour.EnableRifle = m_enableRifle;
                m_gameObject.GetComponent<MaterialBehaviour>().Color = m_color;
                ReachTarget();
            }
        }

    }
}