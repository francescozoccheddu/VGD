using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager : MovementController.IFlushTarget
    {

        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;

        void MovementController.IFlushTarget.FlushCombined(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            Debug.Log("CombinedFlush");
        }

        void MovementController.IFlushTarget.FlushSight(int _step, in Sight _sight)
        {
        }

        void MovementController.IFlushTarget.FlushSimulation(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in SimulationStep _simulation)
        {

        }

        private void StartLocalPlayer()
        {
            m_movementController.StartAt(RoomTime.Now, TimeStep.zero);
        }

        private void UpdateLocalPlayer()
        {
            m_movementController.Update();
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

    }

}
