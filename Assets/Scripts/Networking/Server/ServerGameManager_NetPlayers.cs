using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        #region Private Classes

        private sealed class NetPlayer : AuthoritativePlayer, MovementValidator.ITarget, ActionValidator.ITarget
        {
            #region Public Properties

            public float AverageNotifyInterval => m_notifyTapper.AverageInterval;
            public override bool IsLocal => false;
            public double MaxValidationDelay { get => m_maxValidationDelay; set { Debug.Assert(value >= 0.0); m_actionValidator.MaxDelay = value; m_maxValidationDelay = value; } }
            public NetworkManager.Peer Peer { get; }

            #endregion Public Properties

            #region Private Fields

            private const int c_maxCorrectionFrequency = 5;
            private const double c_maxValidationAnticipation = 4.0f;

            private const float c_notifyDelaySmoothQuickness = 0.2f;
            private readonly ActionValidator m_actionValidator;
            private readonly MovementValidator m_movementValidator;
            private readonly TimeConstants.Tapper m_notifyTapper;
            private double m_maxValidationDelay;
            private float m_timeSinceLastCorrection;
            private int m_lastNotifyStep;
            private bool m_wasAlive;

            #endregion Private Fields

            #region Public Constructors

            public NetPlayer(ServerGameManager _manager, byte _id, NetworkManager.Peer _peer, OffenseBackstage _offenseBackstage) : base(_manager, _id, _offenseBackstage)
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
            }

            #endregion Public Constructors

            #region Public Methods

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

            void ActionValidator.ITarget.Kaze(double _time, KazeInfo _info)
            {
                PutKaze(LocalTime, _info);
            }

            void ActionValidator.ITarget.Shoot(double _time, ShotInfo _info)
            {
                PutShot(LocalTime, _info);
            }

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

            void MovementValidator.ITarget.Validated(int _step, in InputStep _input, in CharacterController _simulation)
            {
                PutInput(_step, _input);
                PutSimulation(_step, _simulation);
            }

            #endregion Public Methods

            #region Protected Methods

            protected override int GetLastValidMovementStep()
            {
                return m_movementValidator.Step;
            }

            protected override void OnActorSpawned()
            {
                base.OnActorSpawned();
                int step = LocalTime.SimulationSteps();
                m_movementValidator.SkipTo(step, false);
                m_movementValidator.Teleport(this.GetSnapshot(step.SimulationPeriod()).simulation);
                m_movementValidator.IsRunning = true;
            }

            protected override void OnUpdated()
            {
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

            protected override void SendReplication(NetworkManager.SendMethod _method)
            {
                m_manager.SendAllBut(Peer, _method);
            }

            protected override void OnActorBreathed()
            {
                base.OnActorBreathed();
                int validationStep = (m_manager.m_time - m_maxValidationDelay).SimulationSteps();
                m_movementValidator.UpdateUntil(validationStep);
            }

            #endregion Protected Methods

            #region Private Methods

            private void UpdateNotifyTapper()
            {
                bool isAlive = LifeHistory.IsAlive(m_manager.m_time);
                if (isAlive && !m_wasAlive)
                {
                    m_notifyTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }

            #endregion Private Methods
        }

        #endregion Private Classes
    }
}