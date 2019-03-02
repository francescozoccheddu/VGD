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

    internal interface PlayerEventListener
    {

        void Moved(Player player /* ... */);

    }

    internal class Player
    {

        public PlayerStats stats;

        protected readonly GameObject m_gameObject;
        protected readonly PlayerBehaviour m_movement;

        protected bool m_isDestroyed;

        public bool IsDestroyed => m_isDestroyed || (m_gameObject == null);

        public readonly PlayerEventListener host;

        public Player(PlayerEventListener _host)
        {
            host = _host;
            m_isDestroyed = false;
            m_gameObject = Object.Instantiate(ScriptManager.Actors.player);
            m_movement = m_gameObject.GetComponent<PlayerBehaviour>();
        }

        public void Move()
        {
            if (!IsDestroyed)
            {

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

    }

    internal sealed class NetPlayer : Player
    {

        public NetPlayer(PlayerEventListener _host) : base(_host)
        {
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
