using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay
{

    internal sealed class PlayerView
    {

        private GameObject m_gameObject;

        private void EnsureSpawned()
        {
            if (m_gameObject == null)
            {
                m_gameObject = Object.Instantiate(ScriptManager.Actors.player);
            }
        }

        public void Move(in Snapshot _snapshot)
        {
            EnsureSpawned();
            m_gameObject.transform.position = _snapshot.simulation.position;
            m_gameObject.transform.eulerAngles = new Vector3(0.0f, _snapshot.sight.Turn, 0.0f);
        }

        public void Spawn()
        {

        }

        public void Die()
        {

        }

    }

}
