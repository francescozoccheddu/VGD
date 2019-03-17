﻿using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        private sealed class NetPlayer
        {

            private const float c_historyOffset = 0.5f;
            private const float c_historyCache = 2.0f;
            public readonly int id;
            private readonly MovementHistory m_movementHistory;
            private readonly PlayerView m_playerView;
            private readonly ClientGameManager m_manager;

            public NetPlayer(ClientGameManager _manager, int _id)
            {
                m_manager = _manager;
                id = _id;
                m_movementHistory = new MovementHistory();
                m_playerView = new PlayerView
                {
                    isSightInterpolationEnabled = false
                };
            }


            public void Update()
            {
                Snapshot snapshot = new Snapshot();
                m_movementHistory.GetSimulation(m_manager.m_time - c_historyOffset, out SimulationStep? simulation, true);
                if (simulation != null)
                {
                    snapshot.simulation = simulation.Value;
                }
                m_movementHistory.GetSight(m_manager.m_time - c_historyOffset, out Sight? sight);
                if (sight != null)
                {
                    snapshot.sight = sight.Value;
                }
                m_movementHistory.ForgetOlder((m_manager.m_time - c_historyCache).SimulationSteps(), true);
                m_playerView.Move(snapshot);
                m_playerView.Update(Time.deltaTime);
            }

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
                    m_movementHistory.Put(step, inputStep);
                    step--;
                }
                m_movementHistory.Put(_step, _snapshot.simulation);
                m_movementHistory.Put(_step, _snapshot.sight);
            }

        }


    }

}
