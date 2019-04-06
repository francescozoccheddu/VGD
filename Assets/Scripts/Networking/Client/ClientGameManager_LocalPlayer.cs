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
            private readonly ClientGameManager m_manager;
            private readonly MovementController m_movementController;
            private int? m_lastNotifiedMovementStep;
            private int m_maxMovementInputStepsSendCount;
            private int m_movementSendRate;
            private float m_timeSinceLastMovementNotify;

            public LocalPlayer(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_manager = _manager;
                m_movementController = new MovementController()
                {
                    target = this
                };
                m_actionController = new ActionController
                {
                    Target = this
                };
            }

            public override bool IsLocal => true;
            public int MaxMovementInputStepsNotifyCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }
            public int MaxMovementNotifyFrequency { get => m_movementSendRate; set { Debug.Assert(value >= 0); m_movementSendRate = value; } }

            public void Correct(int _step, SimulationStepInfo _info)
            {
                SimulationStep correctedSimulation = CorrectSimulation(_step, _info.input, _info.simulation);
                m_movementController.Teleport(new Snapshot { sight = m_movementController.RawSnapshot.sight, simulation = correctedSimulation }, false);
            }

            public void PutDamage(double _time, DamageInfo _info, int _health)
            {
                PutDamage(_time, _info);
                PutHealth(_time, _health);
            }

            public new void PutHitConfirm(double _time, HitConfirmInfo _info)
            {
                base.PutHitConfirm(_time, _info);
            }

            protected override void OnUpdated()
            {
                if (ActionHistoryQuery.IsAlive(m_LocalTime))
                {
                    if (!m_movementController.IsRunning)
                    {
                        m_movementController.StartAt(m_LocalTime);
                    }
                }
                else
                {
                    m_movementController.Pause();
                }
                m_movementController.UpdateUntil(m_LocalTime);
                m_actionController.Update(ActionHistoryLocalTimeQuery, GetSnapshot(m_LocalTime));
                m_timeSinceLastMovementNotify += Time.deltaTime;
                if (m_lastNotifiedMovementStep == null || (m_lastNotifiedMovementStep < m_movementController.Step && m_timeSinceLastMovementNotify >= 1.0f / m_movementSendRate))
                {
                    NotifyMovement();
                }
            }

            #region MovementController.ICommitTarget

            void MovementController.ICommitTarget.Commit(int _step, InputStep _input, Snapshot _snapshot)
            {
                PutInput(_step, _input);
                PutSight(_step, _snapshot.sight);
                PutSimulation(_step, _snapshot.simulation);
            }

            #endregion MovementController.ICommitTarget

            #region ActionController.ITarget

            void ActionController.ITarget.Kaze()
            {
                Serializer.WriteKazeNotify(m_LocalTime);
                m_manager.m_server.Send(NetworkManager.SendMethod.Unreliable);
            }

            void ActionController.ITarget.Shoot(ShotInfo _info)
            {
                PutShoot(m_LocalTime, _info);
                Serializer.WriteShootNotify(m_LocalTime, _info);
                m_manager.m_server.Send(NetworkManager.SendMethod.Unreliable);
            }

            #endregion ActionController.ITarget

            private void NotifyMovement()
            {
                m_timeSinceLastMovementNotify = 0.0f;
                int maxStepsCount = MaxMovementInputStepsNotifyCount;
                if (m_lastNotifiedMovementStep != null)
                {
                    maxStepsCount = Math.Min(maxStepsCount, m_movementController.Step - m_lastNotifiedMovementStep.Value);
                }
                m_lastNotifiedMovementStep = m_movementController.Step;
                IEnumerable<InputStep> inputSteps = GetReversedInputSequence(m_movementController.Step, maxStepsCount);
                Serializer.WriteMovementNotify(m_movementController.Step, inputSteps, m_movementController.RawSnapshot);
                m_manager.m_server.Send(NetworkManager.SendMethod.Unreliable);
            }
        }
    }
}