using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        private sealed class NetPlayer : Player, MovementValidator.ICorrectionTarget, MovementValidator.IValidationTarget, ActionValidator.ITarget
        {
            private const int c_maxCorrectionFrequency = 5;
            private const double c_movementValidatorDuration = 2.0f;

            private readonly ActionValidator m_actionValidator;
            private readonly MovementValidator m_movementValidator;
            private float m_timeSinceLastCorrection;

            public NetPlayer(ServerGameManager _manager, byte _id, NetworkManager.Peer _peer) : base(_manager, _id)
            {
                Peer = _peer;
                m_movementValidator = new MovementValidator(2.0)
                {
                    correctionTarget = this,
                    validationTarget = this,
                    MaxTrustedSteps = 10
                };
                m_actionValidator = new ActionValidator
                {
                    Target = this,
                    MaxAnticipation = 1.0
                };
            }

            public bool IsStarted { get; private set; }

            public NetworkManager.Peer Peer { get; }

            public void Start()
            {
                if (!IsStarted)
                {
                    IsStarted = true;
                    m_movementValidator.SkipTo(m_manager.m_time.SimulationSteps(), false);
                    m_movementValidator.IsRunning = true;
                }
            }

            public void TryKaze(double _time)
            {
                m_actionValidator.PutKaze(_time);
            }

            public void TryMove(int _step, IEnumerable<InputStep> _inputSteps, Snapshot _snapshot)
            {
                m_movementValidator.Put(_step, _inputSteps, _snapshot.simulation);
                PutSight(_step, _snapshot.sight);
            }

            public void TryShoot(double _time, ShotInfo _info)
            {
                m_actionValidator.PutShot(_time, _info);
            }

            public override void Update()
            {
                m_timeSinceLastCorrection += Time.deltaTime;
                m_actionHistory.Update(m_manager.m_time);
                m_movementValidator.UpdateUntil(m_manager.m_time.SimulationSteps());
                UpdateView();
                m_actionHistory.Perform();
                HandleRespawn();
                m_actionValidator.ValidateUntil(m_manager.m_time, m_actionHistory, m_movementHistory, m_inputHistory, Id);
                Trim();
            }

            void MovementValidator.ICorrectionTarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                if (m_timeSinceLastCorrection >= 1.0f / c_maxCorrectionFrequency)
                {
                    m_timeSinceLastCorrection = 0.0f;
                    Serializer.WriteSimulationCorrection(_step, _simulation);
                    Peer.Send(NetworkManager.SendMethod.Unreliable);
                }
            }

            void ActionValidator.ITarget.Kaze(double _time)
            {
                DeathInfo deathInfo = new DeathInfo { isExploded = true, killerId = Id, offenseType = OffenseType.Kaze };
                m_actionHistory.PutDeath(m_manager.m_time, deathInfo);
            }

            void MovementValidator.ICorrectionTarget.Rejected(int _step, bool _newer)
            {
            }

            void ActionValidator.ITarget.Shoot(double _time, ShotInfo _info)
            {
                m_actionHistory.PutShot(m_manager.m_time, _info);
            }

            void MovementValidator.IValidationTarget.Validated(int _step, in InputStep _input, in SimulationStep _simulation)
            {
                m_inputHistory.Put(_step, _input);
                PutSimulation(_step, _simulation);
            }

            protected override void SendReplication()
            {
                m_manager.SendAllBut(Peer, NetworkManager.SendMethod.Unreliable);
            }
        }
    }
}