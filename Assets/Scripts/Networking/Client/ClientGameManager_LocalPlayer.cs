using System;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager : MovementController.ICommitTarget
    {

        private const float c_controllerOffset = 0.5f;
        private const int c_controllerSendFrequency = 10;

        // Components
        private readonly MovementController m_localMovementController;
        private readonly InputHistory m_localInputHistory;
        private readonly PlayerView m_localPlayerView;
        private readonly LinkedListHistory<double, PlayerStats> m_localStatsHistory;
        private readonly ActionHistory m_localActionHistory;
        // Replication
        private int m_lastSendStep;
        private float m_timeSinceLastSend;

        #region MovementController.ICommitTarget

        void MovementController.ICommitTarget.Commit(int _step, InputStep _input)
        {
            m_localInputHistory.Put(_step, _input);
        }

        void MovementController.ICommitTarget.Cut(int _oldest)
        {
            m_localInputHistory.Cut(_oldest);
        }

        #endregion

        private void ScheduleLocalPlayerSend()
        {
            m_lastSendStep = -1;
        }

        private void SyncLocalPlayer(double _time, PlayerStats _stats, int _health)
        {
            m_localStatsHistory.Set(_time, _stats);
            m_localActionHistory.PutHealth(_time, _health);
        }

        private void UpdateLocalPlayer()
        {
            m_localActionHistory.Update(m_time + c_controllerOffset);
            m_localMovementController.UpdateUntil(m_time + c_controllerOffset);
            m_localPlayerView.Move(m_localMovementController.ViewSnapshot);
            m_localPlayerView.isAlive = m_localActionHistory.IsAlive;
            m_localPlayerView.Update(Time.deltaTime);
            // Replicate
            m_timeSinceLastSend += Time.deltaTime;
            if (m_lastSendStep == -1 || (m_lastSendStep < m_localMovementController.Step && m_timeSinceLastSend >= 1.0f / c_controllerSendFrequency))
            {
                m_timeSinceLastSend = 0.0f;
                m_lastSendStep = m_localMovementController.Step;
                m_localInputHistory.PullReverseInputBuffer(m_localMovementController.Step, m_inputBuffer, out int inputStepsCount);
                m_localInputHistory.Clear();
                Serializer.WriteMovementNotifyMessage(m_localMovementController.Step, new ArraySegment<InputStep>(m_inputBuffer, 0, inputStepsCount), m_localMovementController.RawSnapshot);
                m_server.Send(NetworkManager.SendMethod.Unreliable);
            }
            // Trim
            m_localInputHistory.Trim((m_time - 100).SimulationSteps());
            m_localActionHistory.Trim(m_time - 1.0);
        }

    }

}
