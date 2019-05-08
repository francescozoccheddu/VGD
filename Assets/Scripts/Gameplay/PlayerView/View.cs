using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.PlayerView
{
    internal sealed class View
    {
        public ELifeState State { get; set; }

        public bool IsLocal
        {
            get => m_isLocal;
            set
            {
                if (m_gameObject != null)
                {
                    m_cameraBehaviour.SetLocal(value);
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
        private CameraBehaviour m_cameraBehaviour;
        private SightBehaviour m_sightBehaviour;
        private DamperBehaviour m_damperBehaviour;
        private RifleDisplayBehaviour m_rifleDisplayBehaviour;
        private Animator m_animator;

        private ELifeState m_lastState;
        private CharacterController m_simulation;
        private Sight m_sight;
        private bool m_isLocal;
        private GameObject m_explosion;
        private GameObject m_head;
        private Color m_color;

        public View()
        {
            m_lastState = State = ELifeState.Unknown;
            isPositionInterpolationEnabled = false;
            isSightInterpolationEnabled = false;
            positionInterpolationQuickness = 20.0f;
            sightInterpolationQuickness = 5.0f;
            m_isLocal = false;
            m_head = Scripts.PlayerPreferences.heads[0].prefab;
        }

        public void Move(in Snapshot _snapshot)
        {
            m_simulation = _snapshot.simulation;
            m_sight = _snapshot.sight;
        }

        public void ShootRocket() => m_animator?.SetTrigger("Shoot Rocket");

        public void ShootRifle()
        {
            m_animator?.SetTrigger("Shoot Rifle");
            m_rifleDisplayBehaviour?.Shoot(RiflePower);
        }
        public void ReachTarget()
        {
            if (m_gameObject != null)
            {
                ReachSightTarget();
                ReachSimulationTarget();
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
                    m_damperBehaviour.height = Mathf.Lerp(m_damperBehaviour.height, m_simulation.Height, lerpAlpha);
                }
                else
                {
                    ReachSimulationTarget();
                }
                // Sight
                if (isSightInterpolationEnabled)
                {
                    float lerpAlpha = Mathf.Min(0.9f, _deltaTime * sightInterpolationQuickness);
                    m_sightBehaviour.Turn = Mathf.Lerp(m_sightBehaviour.Turn, m_sight.Turn, lerpAlpha);
                    m_sightBehaviour.LookUp = Mathf.Lerp(m_sightBehaviour.LookUp, m_sight.LookUp, lerpAlpha);
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
            m_cameraBehaviour = null;
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
                m_damperBehaviour.height = m_simulation.Height;
            }
        }

        private void ReachSightTarget()
        {
            if (m_gameObject != null)
            {
                m_sightBehaviour.Turn = m_sight.Turn;
                m_sightBehaviour.LookUp = m_sight.LookUp;
                m_sightBehaviour.ReachTarget();
            }
        }
        private void EnsureSpawned()
        {
            if (m_gameObject == null)
            {
                m_gameObject = Object.Instantiate(Scripts.Actors.player, m_simulation.Position, Quaternion.identity);
                m_cameraBehaviour = m_gameObject.GetComponent<CameraBehaviour>();
                m_damperBehaviour = m_gameObject.GetComponent<DamperBehaviour>();
                m_sightBehaviour = m_gameObject.GetComponent<SightBehaviour>();
                m_deathBehaviour = m_gameObject.GetComponent<DeathBehaviour>();
                m_gameObject.GetComponent<MaterialBehaviour>().Color = m_color;
                m_gameObject.GetComponent<HeadBehaviour>().SetHead(m_head);
                m_rifleDisplayBehaviour = m_gameObject.GetComponent<RifleDisplayBehaviour>();
                m_rifleDisplayBehaviour.Power = RiflePower;
                m_rifleDisplayBehaviour.BaseColor = m_color;
                m_animator = m_gameObject.GetComponent<Animator>();
                m_cameraBehaviour.SetLocal(IsLocal);
                ReachTarget();
            }
        }

    }
}