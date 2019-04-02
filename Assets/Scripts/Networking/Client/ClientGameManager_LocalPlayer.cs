using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private sealed class LocalPlayer : Player, MovementController.ICommitTarget
        {
            private readonly MovementController m_movementController;
            private double m_controllerOffset;
            private int? m_lastNotifiedMovementStep;
            private int m_maxMovementInputStepsSendCount;
            private int m_movementSendRate;
            private float m_timeSinceLastMovementNotify;

            public LocalPlayer(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_movementController = new MovementController()
                {
                    target = this
                };
            }

            public double ControllerOffset { get => m_controllerOffset; set { Debug.Assert(value >= 0.0); m_controllerOffset = value; } }

            public void EnsureStarted()
            {
                if (!m_movementController.IsRunning)
                {
                    m_movementController.StartAt(m_manager.m_time + m_controllerOffset);
                }
            }

            public int MaxMovementInputStepsNotifyCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }
            public int MaxMovementNotifyFrequency { get => m_movementSendRate; set { Debug.Assert(value >= 0); m_movementSendRate = value; } }

            public void Correct(int _step, SimulationStepInfo _info)
            {
                m_inputHistory.Put(_step, _info.input);
                SimulationStep correctedSimulation = m_inputHistory.SimulateFrom(_step, _info.simulation);
                m_movementController.Teleport(new Snapshot { sight = m_movementController.RawSnapshot.sight, simulation = correctedSimulation }, false);
            }

            public override void Update()
            {
                double localTime = m_manager.m_time + m_controllerOffset;
                m_movementController.UpdateUntil(localTime);
                UpdateView(localTime, m_movementController.ViewSnapshot);
                m_timeSinceLastMovementNotify += Time.deltaTime;
                if (m_lastNotifiedMovementStep == null || (m_lastNotifiedMovementStep < m_movementController.Step && m_timeSinceLastMovementNotify >= 1.0f / m_movementSendRate))
                {
                    NotifyMovement();
                }
                Trim();
            }

            void MovementController.ICommitTarget.Commit(int _step, InputStep _input, Snapshot _snapshot)
            {
                m_inputHistory.Put(_step, _input);
            }

            void MovementController.ICommitTarget.Cut(int _oldest)
            {
                m_inputHistory.Cut(_oldest);
            }

            private void NotifyMovement()
            {
                m_timeSinceLastMovementNotify = 0.0f;
                int maxStepsCount = MaxMovementInputStepsNotifyCount;
                if (m_lastNotifiedMovementStep != null)
                {
                    maxStepsCount = Math.Min(maxStepsCount, m_movementController.Step - m_lastNotifiedMovementStep.Value);
                }
                m_lastNotifiedMovementStep = m_movementController.Step;
                IEnumerable<InputStep> inputSteps = m_inputHistory.GetReversedInputSequence(m_movementController.Step, maxStepsCount);
                Serializer.WriteMovementNotify(m_movementController.Step, inputSteps, m_movementController.RawSnapshot);
                m_manager.m_server.Send(NetworkManager.SendMethod.Unreliable);
            }
        }
    }
}