using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.PlayerView
{
    internal sealed class View
    {
        #region Public Properties

        public LifeState State { get; set; }

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

        public float RiflePower { get; set; }

        #endregion Public Properties

        #region Public Fields

        public bool isPositionInterpolationEnabled;
        public bool isSightInterpolationEnabled;
        public float positionInterpolationQuickness;
        public float sightInterpolationQuickness;

        #endregion Public Fields

        #region Private Fields

        private GameObject m_gameObject;
        private DeathBehaviour m_deathBehaviour;
        private CameraBehaviour m_cameraBehaviour;
        private SightBehaviour m_sightBehaviour;
        private DamperBehaviour m_damperBehaviour;
        private RifleDisplayBehaviour m_rifleDisplayBehaviour;
        private Animator m_animator;

        private LifeState m_lastState;
        private CharacterController m_simulation;
        private Sight m_sight;
        private bool m_isLocal;
        private GameObject m_explosion;

        #endregion Private Fields

        #region Public Constructors

        public View()
        {
            m_lastState = State = LifeState.Dead;
            isPositionInterpolationEnabled = false;
            isSightInterpolationEnabled = false;
            positionInterpolationQuickness = 4.0f;
            sightInterpolationQuickness = 2.0f;
            m_isLocal = false;
        }

        #endregion Public Constructors

        #region Public Methods

        public void Move(in Snapshot _snapshot)
        {
            m_simulation = _snapshot.simulation;
            m_sight = _snapshot.sight;
        }

        public void ShootRocket()
        {
            m_animator?.SetTrigger("Shoot Rocket");
        }

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
            EnsureSpawned();

            // Life
            if (m_lastState != LifeState.Exploded && State == LifeState.Exploded && m_explosion == null)
            {
                m_explosion = Object.Instantiate(ScriptManager.Actors.explosion, m_simulation.Position, Quaternion.identity);
            }
            m_lastState = State;

            if (State != LifeState.Alive && !m_deathBehaviour.IsDead)
            {
                m_deathBehaviour.Die(m_simulation.Velocity);
            }
            else if (State == LifeState.Alive && m_deathBehaviour.IsDead)
            {
                Destroy();
                EnsureSpawned();
            }

            m_rifleDisplayBehaviour.Power = RiflePower;

            // Position
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
                m_sightBehaviour.turn = Mathf.Lerp(m_sightBehaviour.turn, m_sight.Turn, lerpAlpha);
                m_sightBehaviour.lookUp = Mathf.Lerp(m_sightBehaviour.lookUp, m_sight.LookUp, lerpAlpha);
            }
            else
            {
                ReachSightTarget();
            }
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

        #endregion Public Methods

        #region Private Methods

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
                m_sightBehaviour.turn = m_sight.Turn;
                m_sightBehaviour.lookUp = m_sight.LookUp;
            }
        }

        private void EnsureSpawned()
        {
            if (m_gameObject == null)
            {
                m_gameObject = Object.Instantiate(ScriptManager.Actors.player, m_simulation.Position, m_sight.Quaternion);
                m_cameraBehaviour = m_gameObject.GetComponent<CameraBehaviour>();
                m_damperBehaviour = m_gameObject.GetComponent<DamperBehaviour>();
                m_sightBehaviour = m_gameObject.GetComponent<SightBehaviour>();
                m_deathBehaviour = m_gameObject.GetComponent<DeathBehaviour>();
                m_rifleDisplayBehaviour = m_gameObject.GetComponent<RifleDisplayBehaviour>();
                m_animator = m_gameObject.GetComponent<Animator>();
                m_cameraBehaviour.SetLocal(IsLocal);
                ReachTarget();
            }
        }

        #endregion Private Methods
    }
}