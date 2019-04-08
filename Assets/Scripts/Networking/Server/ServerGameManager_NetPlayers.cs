using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        private sealed class NetPlayer : Player, MovementValidator.ITarget, ActionValidator.ITarget
        {
            private const int c_maxCorrectionFrequency = 5;
            private const double c_maxValidationAnticipation = 4.0f;

            private readonly ActionValidator m_actionValidator;
            private readonly MovementValidator m_movementValidator;
            private const float c_notifyDelaySmoothQuickness = 0.2f;
            private double m_maxValidationDelay;
            private float m_timeSinceLastCorrection;

            public float AverageNotifyInterval => m_notifyTapper.AverageInterval;
            private int m_lastNotifyStep;
            private readonly TimeConstants.Tapper m_notifyTapper;
            private bool m_wasAlive;

            public NetPlayer(ServerGameManager _manager, byte _id, NetworkManager.Peer _peer) : base(_manager, _id)
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
                IsStarted = false;
                MaxValidationDelay = 1.0;
                m_lastNotifyStep = -1;
                m_notifyTapper = new TimeConstants.Tapper(0.0f);
            }

            public override bool IsLocal => false;
            public bool IsStarted { get; private set; }
            public double MaxValidationDelay { get => m_maxValidationDelay; set { Debug.Assert(value >= 0.0); m_actionValidator.MaxDelay = value; m_maxValidationDelay = value; } }
            public NetworkManager.Peer Peer { get; }

            public void Start()
            {
                if (!IsStarted)
                {
                    IsStarted = true;

                    m_ShouldHandleRespawn = true;
                }
            }

            public void TryKaze(double _time, KazeInfo _info)
            {
                m_actionValidator.PutKaze(_time, _info);
            }

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

            public void TryShoot(double _time, ShotInfo _info)
            {
                m_actionValidator.PutShot(_time, _info);
            }

            protected override int GetLastValidMovementStep()
            {
                return m_movementValidator.Step;
            }

            protected override void OnDamageScheduled(double _time, DamageInfo _info)
            {
                Serializer.WriteDamageOrder(_time, _info, (byte) ActionHistory.GetHealth(_time));
                Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnHitConfirmScheduled(double _time, HitConfirmInfo _info)
            {
                Serializer.WriteHitConfirmOrder(_time, _info);
                Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnSpawn(double _time, Snapshot _snapshot)
            {
                m_movementValidator.SkipTo(_time.SimulationSteps(), false);
                m_movementValidator.Teleport(_snapshot.simulation);
            }

            private void UpdateNotifyTapper()
            {
                bool isAlive = ActionHistory.IsAlive(m_manager.m_time);
                if (isAlive && !m_wasAlive)
                {
                    m_notifyTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }

            protected override void OnUpdated()
            {
                UpdateNotifyTapper();
                int validationStep = (m_manager.m_time - m_maxValidationDelay).SimulationSteps();
                if (IsStarted)
                {
                    if (ActionHistory.IsAlive(validationStep.SimulationPeriod()))
                    {
                        if (!m_movementValidator.IsRunning)
                        {
                            m_movementValidator.SkipTo(validationStep, false);
                            m_movementValidator.IsRunning = true;
                        }
                    }
                    else
                    {
                        m_movementValidator.IsRunning = false;
                    }
                }
                m_timeSinceLastCorrection += Time.deltaTime;
                m_movementValidator.UpdateUntil(validationStep);
                double lastMovementTime = m_movementValidator.Step.SimulationPeriod();
                m_actionValidator.ValidateUntil(lastMovementTime, this);
            }

            #region ActionValidator.ITarget

            void ActionValidator.ITarget.Kaze(double _time, KazeInfo _info)
            {
                DeathInfo deathInfo = new DeathInfo { isExploded = true, killerId = Id, offenseType = OffenseType.Explosion, position = _info.position };
                PutDeath(LocalTime, deathInfo);
            }

            void ActionValidator.ITarget.Shoot(double _time, ShotInfo _info)
            {
                PutShoot(LocalTime, _info);
            }

            #endregion ActionValidator.ITarget

            #region MovementValidator.ITarget

            void MovementValidator.ITarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                if (m_timeSinceLastCorrection >= 1.0f / c_maxCorrectionFrequency)
                {
                    m_timeSinceLastCorrection = 0.0f;
                    Serializer.WriteSimulationCorrection(_step, _simulation);
                    Peer.Send(NetworkManager.SendMethod.Unreliable);
                }
            }

            void MovementValidator.ITarget.Rejected(int _step, bool _newer)
            {
            }

            void MovementValidator.ITarget.Validated(int _step, in InputStep _input, in SimulationStep _simulation)
            {
                PutInput(_step, _input);
                PutSimulation(_step, _simulation);
            }

            #endregion MovementValidator.ITarget

            protected override void SendReplication(NetworkManager.SendMethod _method)
            {
                m_manager.SendAllBut(Peer, _method);
            }
        }
    }
}