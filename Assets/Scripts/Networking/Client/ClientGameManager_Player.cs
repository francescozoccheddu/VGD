using UnityEngine;
using Wheeled.Gameplay.Action;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private abstract class Player : PlayerBase
        {
            protected readonly ClientGameManager m_manager;
            private double m_historyDuration;

            protected Player(ClientGameManager _manager, byte _id) : base(_id)
            {
                m_manager = _manager;
            }

            public double HistoryDuration { get => m_historyDuration; set { Debug.Assert(value >= 0.0); m_historyDuration = value; } }

            public void Die(double _time, DeathInfo _info, int _deaths)
            {
                m_actionHistory.PutDeath(_time, _info);
                m_actionHistory.PutDeaths(_time, _deaths);
            }

            public void Spawn(double _time, SpawnInfo _info)
            {
                m_actionHistory.PutSpawn(_time, _info);
            }

            protected void Trim()
            {
                Trim(m_manager.m_time - m_historyDuration);
            }
        }
    }
}