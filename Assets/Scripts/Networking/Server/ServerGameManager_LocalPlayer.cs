﻿using System;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager : MovementController.ICommitTarget
    {

        private readonly MovementController m_movementController;
        private readonly InputHistory m_inputHistory;
        private readonly PlayerView m_view;
        private readonly ActionHistory m_actionHistory;
        private int m_localLastSentStep;

        private void StartLocalPlayer()
        {
            m_movementController.target = this;
            m_movementController.StartAt(m_time);
        }

        private void UpdateLocalPlayer()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                m_actionHistory.Put(m_time + 1.0, new DieAction());
            }
            m_actionHistory.GetSpawnState(m_time, out bool isAlive, out double timeSinceLastStateChange);
            if (!isAlive && timeSinceLastStateChange > c_respawnWaitTime && !m_actionHistory.IsSpawnScheduled(m_time))
            {
                m_actionHistory.Put(m_time + 1.0, new SpawnAction());
            }
            m_movementController.UpdateUntil(m_time);
            m_view.isAlive = isAlive;
            m_view.Move(m_movementController.ViewSnapshot);
            m_view.Update(Time.deltaTime);
            m_inputHistory.Trim((m_time - 100).SimulationSteps());
            m_actionHistory.Trim(m_time - 100);
        }

        private void ReplicateLocalPlayer(bool _force, bool _sendInput)
        {
            if (_force || m_localLastSentStep < m_movementController.Step)
            {
                m_localLastSentStep = m_movementController.Step;
                if (_sendInput)
                {
                    m_inputHistory.PullReverseInputBuffer(m_movementController.Step, m_inputStepBuffer, out int inputStepCount);
                    Serializer.WriteMovementAndInputReplicationMessage(0, m_movementController.Step, new ArraySegment<InputStep>(m_inputStepBuffer, 0, inputStepCount), m_movementController.RawSnapshot);
                    m_inputHistory.Clear();
                }
                else
                {
                    Serializer.WriteMovementReplicationMessage(0, m_movementController.Step, m_movementController.RawSnapshot);
                }
                SendAll(NetworkManager.SendMethod.Unreliable);
            }

        }

        void MovementController.ICommitTarget.Commit(int _step, InputStep _input)
        {
            m_inputHistory.Put(_step, _input);
        }

        void MovementController.ICommitTarget.Cut(int _oldest)
        {
            m_inputHistory.Cut(_oldest);
        }
    }
}