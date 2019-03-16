using System;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager
    {

        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;
        private int m_localLastSentStep;

        private void StartLocalPlayer()
        {
            m_movementController.StartAt(m_time);
        }

        private void UpdateLocalPlayer()
        {
            m_movementController.UpdateUntil(m_time);
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

        private void ReplicateLocalPlayer(bool _force, bool _sendInput)
        {
            int currentStep = m_movementController.Time.SimulationSteps();
            if (_force || m_localLastSentStep < currentStep)
            {
                {
                    m_localLastSentStep = currentStep;
                    if (_sendInput)
                    {
                        m_movementController.PullReversedInputBuffer(m_inputStepBuffer, out int inputStepCount);
                        m_movementController.ClearInputBuffer();
                        Serializer.WriteMovementAndInputReplicationMessage(0, currentStep, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepCount), m_movementController.RawSnapshot);
                    }
                    else
                    {
                        Serializer.WriteMovementReplicationMessage(0, currentStep, m_movementController.RawSnapshot);
                    }
                    SendAll(NetworkManager.SendMethod.Unreliable);
                }
            }

        }

    }
}