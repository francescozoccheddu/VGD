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
            m_movementController.StartAt(RoomTime.Now);
        }

        private void UpdateLocalPlayer()
        {
            m_movementController.UpdateUntil(RoomTime.Now);
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

        private void ReplicateLocalPlayer(bool _force, bool _sendInput)
        {
            if (_force || m_localLastSentStep < m_movementController.Time.Step)
            {
                {
                    m_localLastSentStep = m_movementController.Time.Step;
                    if (_sendInput)
                    {
                        m_movementController.PullReversedInputBuffer(m_inputStepBuffer, out int inputStepCount);
                        m_movementController.ClearInputBuffer();
                        Serializer.WriteMovementAndInputReplicationMessage(0, m_movementController.Time.Step, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepCount), m_movementController.RawSnapshot);
                    }
                    else
                    {
                        Serializer.WriteMovementReplicationMessage(0, m_movementController.Time.Step, m_movementController.RawSnapshot);
                    }
                    SendAll(NetworkManager.SendMethod.Unreliable);
                }
            }

        }

    }
}