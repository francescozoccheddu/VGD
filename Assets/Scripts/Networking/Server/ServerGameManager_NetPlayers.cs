using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.Player;

namespace Wheeled.Networking.Server
{
    public sealed partial class ServerGameManager
    {
        private sealed class NetPlayer : AuthoritativePlayer, MovementValidator.ITarget, ActionValidator.ITarget
        {
            public float AverageNotifyInterval => m_notifyTapper.AverageInterval;
            public double MaxValidationDelay { get => m_maxValidationDelay; set { Debug.Assert(value >= 0.0); m_actionValidator.MaxDelay = value; m_maxValidationDelay = value; } }
            public NetworkManager.Peer Peer { get; }

            private const int c_maxCorrectionFrequency = 5;
            private const float c_maxStartWaitTime = 5.0f;
            private const double c_maxValidationAnticipation = 4.0f;

            private const float c_notifyDelaySmoothQuickness = 0.2f;
            private readonly ActionValidator m_actionValidator;
            private readonly MovementValidator m_movementValidator;
            private readonly TimeConstants.Tapper m_notifyTapper;
            private double m_maxValidationDelay;
            private float m_timeSinceLastCorrection;
            private int m_lastNotifyStep;
            private bool m_wasAlive;
            private readonly float m_creationTime;
            private const float c_playerIntroductionResendPeriod = 20.0f;
            private float m_lastPlayerIntroduction;

            public NetPlayer(ServerGameManager _manager, int _id, NetworkManager.Peer _peer, OffenseBackstage _offenseBackstage) : base(_manager, _id, _offenseBackstage, false)
            {
                Peer = _peer;
                m_movementValidator = new MovementValidator(c_maxValidationAnticipation)
                {
                    Target = this,
                    MaxTrustedSteps = 10
                };
                m_actionValidator = new ActionValidator
                {
                    Target = this,
                    MaxAnticipation = c_maxValidationAnticipation
                };
                MaxValidationDelay = 1.0;
                m_lastNotifyStep = -1;
                m_notifyTapper = new TimeConstants.Tapper(0.0f);
                m_creationTime = Time.realtimeSinceStartup;
                m_lastPlayerIntroduction = Time.realtimeSinceStartup;
            }

            public void TryKaze(double _time, KazeInfo _info) => m_actionValidator.PutKaze(_time, _info);

            public void TryMove(int _step, IEnumerable<InputStep> _inputSteps, Snapshot _snapshot)
            {
                if (_step > m_lastNotifyStep && m_wasAlive)
                {
                    m_notifyTapper.Tap();
                    m_lastNotifyStep = _step;
                }
                m_movementValidator.Put(_step, _inputSteps, _snapshot.simulation);
                PutSight(_step, _snapshot.sight);
            }

            public void TryShoot(double _time, ShotInfo _info) => m_actionValidator.PutShot(_time, _info);

            void ActionValidator.ITarget.Kaze(double _time, KazeInfo _info) => PutKaze(LocalTime, _info);

            void ActionValidator.ITarget.Shoot(double _time, ShotInfo _info) => PutShot(LocalTime, _info);

            void MovementValidator.ITarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                if (m_timeSinceLastCorrection >= 1.0f / c_maxCorrectionFrequency)
                {
                    m_timeSinceLastCorrection = 0.0f;
                    Serializer.WriteSimulationCorrection(_step, _simulation);
                    Peer.Send(NetworkManager.ESendMethod.Unreliable);
                }
            }

            void MovementValidator.ITarget.Rejected(int _step, bool _newer)
            {
            }

            void MovementValidator.ITarget.Validated(int _step, in InputStep _input, in CharacterController _simulation)
            {
                PutInput(_step, _input);
                PutSimulation(_step, _simulation);
            }

            protected override int GetLastValidMovementStep() => m_movementValidator.Step;

            protected override void OnActorSpawned()
            {
                base.OnActorSpawned();
                int step = LocalTime.SimulationSteps();
                m_movementValidator.SkipTo(step, false);
                m_movementValidator.Teleport(this.GetSnapshot(step.SimulationPeriod()).simulation);
                m_movementValidator.IsRunning = true;
            }

            public bool ShouldKick => !IsStarted && Time.realtimeSinceStartup - m_creationTime > c_maxStartWaitTime;


            protected override void OnQuitScheduled(double _time)
            {
                base.OnQuitScheduled(_time);
                Peer.Disconnect();
            }

            protected override void OnUpdated()
            {
                if (!IsStarted && Time.realtimeSinceStartup - m_creationTime > c_maxStartWaitTime)
                {
                    Debug.LogFormat("Start message timeout. Kicking player {0}", Id);
                    PutQuit(m_manager.m_time);
                }
                if (Time.realtimeSinceStartup - m_lastPlayerIntroduction > c_playerIntroductionResendPeriod)
                {
                    m_lastPlayerIntroduction = Time.realtimeSinceStartup;
                    m_manager.SendPlayerIntroductions(this, NetworkManager.ESendMethod.Unreliable);
                }
                UpdateNotifyTapper();
                m_timeSinceLastCorrection += Time.deltaTime;
                double lastMovementTime = m_movementValidator.Step.SimulationPeriod();
                m_actionValidator.ValidateUntil(lastMovementTime, this);
                base.OnUpdated();
            }

            protected override void OnActorDied()
            {
                base.OnActorDied();
                m_movementValidator.IsRunning = false;
            }

            protected override void SendReplication(NetworkManager.ESendMethod _method) => m_manager.SendAllBut(Peer, _method);

            protected override void OnActorBreathed()
            {
                base.OnActorBreathed();
                int validationStep = (m_manager.m_time - m_maxValidationDelay).SimulationSteps();
                m_movementValidator.UpdateUntil(validationStep);
            }

            private void UpdateNotifyTapper()
            {
                bool isAlive = LifeHistory.IsAlive(m_manager.m_time);
                if (isAlive && !m_wasAlive)
                {
                    m_notifyTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }
        }
    }
}