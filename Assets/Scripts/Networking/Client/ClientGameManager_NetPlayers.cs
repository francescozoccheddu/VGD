using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        private sealed class NetPlayer
        {

            private const float c_historyOffset = 0.5f;
            private const float c_historyCache = 2.0f;

            private readonly ClientGameManager m_manager;
            // Info
            public readonly int id;
            public PlayerInfo Info { get; private set; }
            public bool IsIntroduced { get; private set; }
            public int Ping { get; private set; }
            private double m_lastSyncTime = double.NegativeInfinity;
            // Components
            private readonly MovementHistory m_movementHistory;
            private readonly InputHistory m_inputHistory;
            private readonly ActionHistory m_actionHistory;
            private readonly PlayerView m_playerView;

            public NetPlayer(ClientGameManager _manager, int _id)
            {
                m_manager = _manager;
                id = _id;
                m_movementHistory = new MovementHistory();
                m_inputHistory = new InputHistory();
                m_actionHistory = new ActionHistory();
                m_playerView = new PlayerView
                {
                    isSightInterpolationEnabled = false
                };
            }

            public void Update()
            {
                Snapshot snapshot = new Snapshot();
                m_movementHistory.GetSimulation(m_manager.m_time - c_historyOffset, out SimulationStep? simulation, m_inputHistory);
                if (simulation != null)
                {
                    snapshot.simulation = simulation.Value;
                }
                m_movementHistory.GetSight(m_manager.m_time - c_historyOffset, out Sight? sight);
                if (sight != null)
                {
                    snapshot.sight = sight.Value;
                }
                m_actionHistory.Update(m_manager.m_time - c_historyOffset);
                m_playerView.isAlive = m_actionHistory.IsAlive;
                m_playerView.Move(snapshot);
                m_playerView.Update(Time.deltaTime);
                // Trim
                int forgetStep = (m_manager.m_time - c_historyCache).SimulationSteps();
                m_movementHistory.ForgetOlder(forgetStep, true);
                m_actionHistory.Trim(forgetStep.SimulationPeriod());
                m_inputHistory.Trim(forgetStep);
            }

            public void Sync(double _time, int _kills, int _deaths, int _health, int _ping)
            {
                m_actionHistory.PutKills(_time, _kills);
                m_actionHistory.PutDeaths(_time, _deaths);
                m_actionHistory.PutHealth(_time, _health);
                if (_time > m_lastSyncTime)
                {
                    m_lastSyncTime = _time;
                    Ping = _ping;
                }
            }

            public void Quit(double _time)
            {
                m_actionHistory.PutQuit(_time);
            }

            public void Introduce(PlayerInfo _info)
            {
                IsIntroduced = true;
                Info = _info;
            }

            public void Move(int _step, Snapshot _snapshot)
            {
                m_movementHistory.Put(_step, _snapshot.simulation);
                m_movementHistory.Put(_step, _snapshot.sight);
            }

            internal void Spawn(double _time)
            {
                m_actionHistory.PutSpawn(_time);
            }

            public void Move(int _step, IEnumerable<InputStep> _reversedInputSteps, Snapshot _snapshot)
            {
                int step = _step;
                foreach (InputStep inputStep in _reversedInputSteps)
                {
                    m_inputHistory.Put(step, inputStep);
                    step--;
                }
                m_movementHistory.Put(_step, _snapshot.simulation);
                m_movementHistory.Put(_step, _snapshot.sight);
            }

        }

    }

}
