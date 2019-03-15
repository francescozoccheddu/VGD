using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager : MovementController.IFlushTarget
    {

        private const float c_controllerOffset = 0.5f;
        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;

        private void UpdateLocalPlayer()
        {
            m_movementController.Update();
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

        #region InteractivePlayer.IFlushTarget

        void MovementController.IFlushTarget.FlushSimulation(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in SimulationStep _simulation)
        {
            Serializer.WriteSimulationMessage(_firstStep, _inputSteps, _simulation);
            m_server.Send(LiteNetLib.DeliveryMethod.Unreliable);
        }

        void MovementController.IFlushTarget.FlushSight(int _step, in Sight _sight)
        {
            Serializer.WriteSightMessage(_step, _sight);
            m_server.Send(LiteNetLib.DeliveryMethod.Unreliable);
        }

        void MovementController.IFlushTarget.FlushCombined(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            Serializer.WriteSimulationAndSightMessage(_firstStep, _inputSteps, _snapshot);
            m_server.Send(LiteNetLib.DeliveryMethod.Unreliable);
        }

        #endregion

    }

}
