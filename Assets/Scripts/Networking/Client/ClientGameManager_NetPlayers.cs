using System.Collections.Generic;

using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private sealed class NetPlayer : Player
        {
            // Components
            private readonly MovementHistory m_movementHistory;

            private double m_historyOffset;

            public NetPlayer(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_movementHistory = new MovementHistory();
            }

            public double HistoryOffset { get => m_historyOffset; set { Debug.Assert(value >= 0.0); m_historyOffset = value; } }

            public void Move(int _step, Snapshot _snapshot)
            {
                m_movementHistory.Put(_step, _snapshot.simulation);
                m_movementHistory.Put(_step, _snapshot.sight);
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

            public override void Update()
            {
                Snapshot snapshot = new Snapshot();
                m_movementHistory.GetSimulation(m_manager.m_time - m_historyOffset, out SimulationStep? simulation, m_inputHistory);
                if (simulation != null)
                {
                    snapshot.simulation = simulation.Value;
                }
                m_movementHistory.GetSight(m_manager.m_time - m_historyOffset, out Sight? sight);
                if (sight != null)
                {
                    snapshot.sight = sight.Value;
                }
                UpdateView(m_manager.m_time - HistoryOffset, snapshot);
                // Trim
                m_movementHistory.ForgetOlder((m_manager.m_time - HistoryDuration).SimulationSteps(), true);
                Trim();
            }
        }
    }
}