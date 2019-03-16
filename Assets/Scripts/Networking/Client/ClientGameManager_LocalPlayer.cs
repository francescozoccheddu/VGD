using System;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        private const float c_controllerOffset = 0.5f;
        private const int c_controllerSendFrequency = 10;
        private readonly MovementController m_movementController;
        private readonly PlayerView m_view;
        private int m_lastSendStep;
        private float m_timeSinceLastSend;

        private void ScheduleLocalPlayerSend()
        {
            m_lastSendStep = -1;
        }

        private void UpdateLocalPlayer()
        {
            m_movementController.UpdateUntil(m_time + c_controllerOffset);
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
            m_timeSinceLastSend += Time.deltaTime;
            if (m_lastSendStep == -1 || (m_lastSendStep < m_movementController.Step && m_timeSinceLastSend >= 1.0f / c_controllerSendFrequency))
            {
                m_timeSinceLastSend = 0.0f;
                m_lastSendStep = m_movementController.Step;
                m_movementController.PullReversedInputBuffer(m_inputBuffer, out int inputStepsCount);
                m_movementController.ClearInputBuffer();
                Serializer.WriteMovementNotifyMessage(m_movementController.Step, new ArraySegment<InputStep>(m_inputBuffer, 0, inputStepsCount), m_movementController.RawSnapshot);
                m_server.Send(NetworkManager.SendMethod.Unreliable);
            }
        }

    }

}
