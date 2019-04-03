using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking
{
    internal abstract class PlayerBase
    {
        protected readonly ActionHistory m_actionHistory;
        protected readonly InputHistory m_inputHistory;
        private const double c_historyDuration = 2.0;
        private readonly PlayerView m_view;
        private double m_lastPingTime = double.NegativeInfinity;

        protected PlayerBase(byte _id)
        {
            Id = _id;
            m_inputHistory = new InputHistory();
            m_actionHistory = new ActionHistory();
            m_view = new PlayerView();
        }

        public byte Id { get; }
        public PlayerInfo? Info { get; private set; }
        public int Ping { get; private set; }

        public void Die(double _time, DeathInfo _info)
        {
            m_actionHistory.PutDeath(_time, _info);
        }

        public void Introduce(PlayerInfo _info)
        {
            Info = _info;
        }

        public void PutPing(double _time, int _ping)
        {
            if (m_lastPingTime > _time == false)
            {
                Ping = _ping;
                m_lastPingTime = _time;
            }
        }

        public void Quit(double _time)
        {
            m_actionHistory.PutQuit(_time);
        }

        public void Sync(double _time, int _kills, int _deaths, int _health)
        {
            m_actionHistory.PutHealth(_time, _health);
            m_actionHistory.PutKills(_time, _kills);
            m_actionHistory.PutDeaths(_time, _deaths);
        }

        public abstract void Update();

        protected void Trim(double _oldestTime)
        {
            m_inputHistory.Trim(_oldestTime.SimulationSteps());
            m_actionHistory.Trim(_oldestTime);
        }

        protected void UpdateView(in Snapshot _snapshot)
        {
            // Update view
            if (m_actionHistory.IsQuit)
            {
                m_view.Destroy();
            }
            else
            {
                m_view.Move(_snapshot);
                m_view.isAlive = m_actionHistory.IsAlive;
                m_view.Update(Time.deltaTime);
            }
        }
    }
}