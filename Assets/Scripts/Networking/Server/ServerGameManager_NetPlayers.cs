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
            private const double c_movementValidatorDuration = 4.0f;

            private readonly ActionValidator m_actionValidator;
            private readonly MovementValidator m_movementValidator;
            private float m_timeSinceLastCorrection;

            public NetPlayer(ServerGameManager _manager, byte _id, NetworkManager.Peer _peer) : base(_manager, _id)
            {
                Peer = _peer;
                m_movementValidator = new MovementValidator(c_movementValidatorDuration)
                {
                    Target = this,
                    MaxTrustedSteps = 10
                };
                m_actionValidator = new ActionValidator
                {
                    Target = this,
                    MaxAnticipation = 1.0
                };
                IsStarted = false;
            }

            public override bool IsLocal => false;
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

            protected override void OnDamageScheduled(double _time, DamageInfo _info)
            {
                Serializer.WriteDamageOrder(_time, _info, (byte) State.Health);
                Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnHitConfirmScheduled(double _time, HitConfirmInfo _info)
            {
                Serializer.WriteHitConfirmOrder(_time, _info);
                Peer.Send(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnUpdated()
            {
                m_timeSinceLastCorrection += Time.deltaTime;
                m_movementValidator.UpdateUntil(m_LocalTime.SimulationSteps());
                m_actionValidator.ValidateUntil(m_LocalTime, State, Snapshot);
            }

            #region ActionValidator.ITarget

            void ActionValidator.ITarget.Kaze(double _time)
            {
                DeathInfo deathInfo = new DeathInfo { isExploded = true, killerId = Id, offenseType = OffenseType.Kaze };
                PutDeath(m_LocalTime, deathInfo);
            }

            void ActionValidator.ITarget.Shoot(double _time, ShotInfo _info)
            {
                PutShoot(m_LocalTime, _info);
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