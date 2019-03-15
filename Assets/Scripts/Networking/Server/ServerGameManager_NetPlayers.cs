using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager
    {

        private sealed class NetPlayer : MovementValidator.ICorrectionTarget, MovementValidator.IValidationTarget
        {

            private const int c_historyCacheSteps = 100;
            private readonly ServerGameManager m_manager;
            private readonly MovementValidator m_movementValidator;
            private readonly MovementHistory m_movementHistory;
            private readonly PlayerView m_view;

            public readonly byte id;
            public readonly NetworkManager.Peer peer;
            public bool IsStarted { get; private set; }

            public NetPlayer(ServerGameManager _manager, byte _id, NetworkManager.Peer _peer)
            {
                m_manager = _manager;
                id = _id;
                peer = _peer;
                m_movementValidator = new MovementValidator(3.0f)
                {
                    correctionTarget = this,
                    validationTarget = this,
                    MaxTrustedSteps = 10,
                    MinCorrectionRate = 3,
                };
                m_movementHistory = new MovementHistory(true);
                m_view = new PlayerView();
            }

            public void Start()
            {
                if (!IsStarted)
                {
                    IsStarted = true;
                    m_movementValidator.StartAt(RoomTime.Now.Step, false);
                }
            }

            public void Move(int _firstStep, IEnumerable<InputStep> _inputSteps, in SimulationStep _simulation)
            {
                m_movementValidator.Put(_firstStep, _inputSteps, _simulation);
            }

            public void Sight(int _step, in Sight _sight)
            {
                m_movementHistory.Put(_step, _sight);
            }

            public void Update()
            {
                m_movementValidator.UpdateUntil(RoomTime.Now.Step);
                m_movementHistory.TrimOlder(RoomTime.Now.Step - c_historyCacheSteps, true);
                Snapshot snapshot = new Snapshot();
                m_movementHistory.GetSimulation(RoomTime.Now, out SimulationStep? simulation);
                if (simulation != null)
                {
                    snapshot.simulation = simulation.Value;
                }
                m_movementHistory.GetSight(RoomTime.Now, out Sight? sight);
                if (sight != null)
                {
                    snapshot.sight = sight.Value;
                }
                m_view.Move(snapshot);
                m_view.Update(Time.deltaTime);
            }

            public void Destroy()
            {
                m_view.Destroy();
            }

            void MovementValidator.ICorrectionTarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                Serializer.WriteSimulationCorrectionMessage(_step, _simulation);
                peer.Send(false);
            }

            void MovementValidator.ICorrectionTarget.Rejected(int _step, bool _newer)
            {
            }

            void MovementValidator.IValidationTarget.Validated(int _step, in InputStep _input, in SimulationStep _simulation)
            {
                m_movementHistory.Put(_step, _input);
                m_movementHistory.Put(_step, _simulation);
            }

        }

    }

}
