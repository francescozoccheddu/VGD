using UnityEngine;
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

    internal struct PlayerBehaviour
    {
        public PlayerMovement movement;
    }

    internal interface PlayerEventListener
    {

        void Moved(Player player /* ... */);

    }

    internal sealed class Player
    {

        public PlayerStats stats;

        private readonly PlayerMovement.History m_movementHistory;

        private GameObject m_gameObject;
        private PlayerBehaviour m_behaviour;

        public bool IsInstantiated => m_gameObject != null;
        public PlayerBehaviour Behaviour => IsInstantiated ? m_behaviour : new PlayerBehaviour();

        public void Instantiate()
        {
            if (!IsInstantiated)
            {
                m_gameObject = Object.Instantiate(GameManager.Instance.pawns.playerPrefab);
                m_behaviour.movement = m_gameObject.GetComponent<PlayerMovement>();
            }
        }

        public void Destroy()
        {
            if (IsInstantiated)
            {
                Object.Destroy(m_gameObject);
                m_gameObject = null;
            }
        }

    }

}
