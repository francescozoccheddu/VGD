using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private sealed class LocalPlayer : Player, MovementController.ICommitTarget, ActionController.ITarget
        {
            private readonly ActionController m_actionController;
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
                m_actionController = new ActionController
                {
                    Target = this
                };
            }

            public double ControllerOffset { get => m_controllerOffset; set { Debug.Assert(value >= 0.0); m_controllerOffset = value; } }
            public int MaxMovementInputStepsNotifyCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }
            public int MaxMovementNotifyFrequency { get => m_movementSendRate; set { Debug.Assert(value >= 0); m_movementSendRate = value; } }
            private double m_LocalTime => m_manager.m_time + m_controllerOffset;

            public void ConfirmHit(double _time, HitConfirmInfo _info, int _kills)
            {
                m_actionHistory.PutHitConfirm(_time, _info);
                m_actionHistory.PutKills(_time, _kills);
            }

            public void Correct(int _step, SimulationStepInfo _info)
            {
                m_inputHistory.Put(_step, _info.input);
                SimulationStep correctedSimulation = m_inputHistory.SimulateFrom(_step, _info.simulation);
                m_movementController.Teleport(new Snapshot { sight = m_movementController.RawSnapshot.sight, simulation = correctedSimulation }, false);
            }

            public void Damage(double _time, DamageInfo _info, int _health)
            {
                m_actionHistory.PutDamage(_time, _info);
                m_actionHistory.PutHealth(_time, _health);
            }

            public void EnsureStarted()
            {
                if (!m_movementController.IsRunning)
                {
                    m_movementController.StartAt(m_manager.m_time + m_controllerOffset);
                }
            }

            public override void Update()
            {
                m_actionHistory.Update(m_LocalTime);
                m_movementController.UpdateUntil(m_LocalTime);
                m_actionController.Update(m_actionHistory, m_movementController.ViewSnapshot);
                UpdateView(m_movementController.ViewSnapshot);
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

            void ActionController.ITarget.Kaze()
            {
            }

            void ActionController.ITarget.Shoot(ShotInfo _info)
            {
                m_actionHistory.PutShot(m_LocalTime, _info);
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