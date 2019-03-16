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
        private float m_timeSinceLastSend;

        private void StartLocalPlayer()
        {
            m_movementController.StartAt(RoomTime.Now);
        }

        private void UpdateLocalPlayer()
        {
            m_timeSinceLastSend += Time.deltaTime;
            m_movementController.UpdateUntil(RoomTime.Now);
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
        }

        private void ScheduleLocalPlayerReplication()
        {
            m_localLastSentStep = -1;
        }

        private void ReplicateLocalPlayer()
        {
            if (m_localLastSentStep == -1 || (m_localLastSentStep < m_movementController.Time.Step && m_timeSinceLastSend > 1.0f / c_replicationRate))
            {
                m_timeSinceLastSend = 0.0f;
                m_localLastSentStep = m_movementController.Time.Step;
                if (c_sendInputReplication)
                {
                    int firstStep = Math.Max(m_localLastSentStep + 1, m_movementController.Time.Step - m_inputStepBuffer.Length + 1);
                    m_movementController.PullInputBuffer(ref firstStep, m_inputStepBuffer, out int inputStepCount);
                    int step = m_movementController.Time.Step - inputStepCount + 1;
                    Serializer.WriteMovementAndInputReplicationMessage(0, firstStep, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepCount), m_movementController.RawSnapshot);
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
