using UnityEngine;
using Wheeled.Assets.Scripts.Core;
using Wheeled.Gameplay;

namespace Wheeled.Core
{

    internal struct PlayerStats
    {

        public int kills;
        public int deaths;
        public int ping;
        public string name;
        public float time;

    }

    internal interface IPlayerEventListener
    {

        void Moved(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation);

        void Corrected(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _simulation);

    }

    internal sealed class Player
    {

        public PlayerStats stats;

        private readonly GameObject m_gameObject;
        private readonly PlayerBehaviour m_behaviour;

        private bool m_isDestroyed;

        public bool IsDestroyed => m_isDestroyed || (m_gameObject == null);

        public Player()
        {
            m_isDestroyed = false;
            m_gameObject = Object.Instantiate(ScriptManager.Actors.player);
            m_behaviour = m_gameObject.GetComponent<PlayerBehaviour>();
        }

        public void Setup(IPlayerEventListener _eventListener, bool _isInteractive, bool _isAuthoritative)
        {
            if (!IsDestroyed)
            {
                m_behaviour.host = _eventListener;
                m_behaviour.isInteractive = _isInteractive;
                m_behaviour.isAuthoritative = _isAuthoritative;
            }
        }

        public void Move(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation)
        {
            if (!IsDestroyed)
            {
                m_behaviour.Move(_node, _input, _calculatedSimulation);
            }
        }

        public void Correct(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _simulation)
        {
            if (!IsDestroyed)
            {
                m_behaviour.Correct(_node, _input, _simulation);
            }
        }

        public void Die()
        {
            if (!IsDestroyed)
            {

            }
        }

        public void Hit()
        {
            if (!IsDestroyed)
            {

            }
        }

        public void Spawn()
        {
            if (!IsDestroyed)
            {

            }
        }

        public void DoPOA(float _ping)
        {
            if (!IsDestroyed)
            {
                m_behaviour.DoPOA(_ping);
            }
        }

        public void Destroy()
        {
            if (!m_isDestroyed)
            {
                m_isDestroyed = true;
                if (m_gameObject != null)
                {
                    Object.Destroy(m_gameObject);
                }
            }
        }
    }

}
