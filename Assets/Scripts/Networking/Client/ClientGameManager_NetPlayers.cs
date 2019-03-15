using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        private sealed class NetPlayer
        {

            public readonly int id;
            private readonly MovementHistory m_movementHistory;
            private readonly PlayerView m_playerView;

            public NetPlayer(int _id)
            {
                id = _id;
                m_movementHistory = new MovementHistory(false);
                m_playerView = new PlayerView();
            }

            public void Update()
            {
                Snapshot snapshot = new Snapshot();
                m_movementHistory.GetSimulation(RoomTime.Now, out SimulationStep? simulation);
                if (simulation != null)
                {
                    snapshot.simulation = simulation.Value;
                }
                m_movementHistory.GetSight(RoomTime.Now, out Sight? sight);
                if (sight != null)
                {
                    snapshot.sight = sight.Value;
                }
                m_movementHistory.TrimOlder(RoomTime.Now.Step - 100, true);
                m_playerView.Move(snapshot);
                m_playerView.Update(Time.deltaTime);
            }

            public void Move(int _step, Snapshot _snapshot)
            {
                m_movementHistory.Put(_step, _snapshot.simulation);
                m_movementHistory.Put(_step, _snapshot.sight);
            }

            public void Move(int _firstStep, IEnumerable<InputStep> _inputSteps, Snapshot _snapshot)
            {
                int step = _firstStep;
                foreach (InputStep inputStep in _inputSteps)
                {
                    m_movementHistory.Put(step, inputStep);
                    step++;
                }
                m_movementHistory.Put(step - 1, _snapshot.simulation);
                m_movementHistory.Put(step - 1, _snapshot.sight);
            }

        }


    }

}
