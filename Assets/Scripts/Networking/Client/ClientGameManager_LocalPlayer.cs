﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Offense;
using Wheeled.Core;

namespace Wheeled.Networking.Client
{
    public sealed partial class ClientGameManager
    {
        private sealed class LocalPlayer : ClientPlayer
        {
            public int MaxMovementInputStepsNotifyCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }
            public int MaxMovementNotifyFrequency { get => m_movementSendRate; set { Debug.Assert(value >= 0); m_movementSendRate = value; } }

            private readonly PlayerController m_playerController;
            private int? m_lastNotifiedMovementStep;
            private int m_maxMovementInputStepsSendCount;
            private int m_movementSendRate;
            private float m_timeSinceLastMovementNotify;

            private readonly ClientGameManager m_manager;

            public LocalPlayer(ClientGameManager _manager, int _id, OffenseBackstage _offenseBackstage) : base(_id, _offenseBackstage, true)
            {
                m_manager = _manager;
                m_playerController = new PlayerController(this);
            }

            public void Correct(int _step, SimulationStepInfo _info)
            {
                PutInput(_step, _info.input);
                m_playerController.Teleport(InputHistory.SimulateFrom(_step, _info.simulation));
            }

            public void PutDamage(double _time, DamageInfo _info, int _health)
            {
                PutDamage(_time, _info);
                PutHealth(_time, _health);
            }

            public void PutHitConfirm(double _time, EOffenseType _info)
            {
                m_playerController.PutHitConfirm(_time, _info);
            }

            protected override void OnActorBreathed()
            {
                base.OnActorBreathed();
                m_playerController.OnActorBreathed();
            }

            protected override void OnDamageScheduled(double _time, DamageInfo _info)
            {
                base.OnDamageScheduled(_time, _info);
                ClientGameManager manager = (ClientGameManager) GameManager.Current;
                Vector3? position = manager.GetPlayerById(_info.offenderId)?.GetSnapshot(_time).simulation.Position;
                m_playerController.OnDamageScheduled(_time, _info, position);
            }

            protected override void OnActorDied()
            {
                base.OnActorDied();
                m_playerController.OnActorDied();
            }

            protected override void OnActorSpawned()
            {
                base.OnActorSpawned();
                m_playerController.OnActorSpawned();
            }

            protected override void OnUpdated()
            {
                base.OnUpdated();
                m_playerController.OnUpdated();
                m_timeSinceLastMovementNotify += Time.deltaTime;
                if (m_lastNotifiedMovementStep == null || (m_lastNotifiedMovementStep < m_playerController.MovementStep && m_timeSinceLastMovementNotify >= 1.0f / m_movementSendRate))
                {
                    NotifyMovement();
                }
            }

            protected override void OnKazeScheduled(double _time, KazeInfo _info)
            {
                base.OnKazeScheduled(_time, _info);
                Serializer.WriteKazeNotify(_time, _info);
                m_manager.m_server.Send(NetworkManager.ESendMethod.ReliableSequenced);
            }

            protected override void OnShotScheduled(double _time, ShotInfo _info)
            {
                base.OnShotScheduled(_time, _info);
                Serializer.WriteShootNotify(_time, _info);
                m_manager.m_server.Send(NetworkManager.ESendMethod.ReliableSequenced);
            }

            protected override void OnInfoSetup()
            {
                base.OnInfoSetup();
                m_playerController.OnInfoSetup();
            }
            private void NotifyMovement()
            {
                m_timeSinceLastMovementNotify = 0.0f;
                int maxStepsCount = MaxMovementInputStepsNotifyCount;
                if (m_lastNotifiedMovementStep != null)
                {
                    maxStepsCount = Math.Min(maxStepsCount, m_playerController.MovementStep - m_lastNotifiedMovementStep.Value);
                }
                m_lastNotifiedMovementStep = m_playerController.MovementStep;
                IEnumerable<InputStep> inputSteps = InputHistory.GetReversedInputSequence(m_playerController.MovementStep, maxStepsCount);
                Serializer.WriteMovementNotify(m_playerController.MovementStep, inputSteps, this.GetSnapshot(m_playerController.MovementStep.SimulationPeriod()));
                m_manager.m_server.Send(NetworkManager.ESendMethod.Unreliable);
            }
        }
    }
}