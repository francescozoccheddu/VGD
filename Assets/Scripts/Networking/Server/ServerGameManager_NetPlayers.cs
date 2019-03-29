using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{

    internal sealed partial class ServerGameManager
    {

        private sealed class NetPlayer : MovementValidator.ICorrectionTarget, MovementValidator.IValidationTarget
        {

            private const int c_maxCorrectionFrequency = 2;
            private const int c_historyCacheSteps = 100;
            private const int c_maxReplicationInputSteps = 10;

            public readonly byte id;
            public readonly NetworkManager.Peer peer;
            private readonly ServerGameManager m_manager;
            public PlayerStats stats;
            public PlayerInfo info;
            public bool IsStarted { get; private set; }
            // Components
            private readonly MovementValidator m_movementValidator;
            private readonly InputHistory m_inputHistory;
            private readonly MovementHistory m_movementHistory;
            private readonly PlayerView m_view;
            private readonly ActionHistory m_actionHistory;
            // Replication
            private float m_timeSinceLastCorrection;
            private int m_lastSendStep;

            public NetPlayer(ServerGameManager _manager, byte _id, NetworkManager.Peer _peer)
            {
                m_manager = _manager;
                id = _id;
                peer = _peer;
                m_inputHistory = new InputHistory();
                m_movementValidator = new MovementValidator(3.0f)
                {
                    correctionTarget = this,
                    validationTarget = this,
                    MaxTrustedSteps = 10,
                };
                m_movementHistory = new MovementHistory();
                m_actionHistory = new ActionHistory();
                m_view = new PlayerView();
            }

            public int GetHealth()
            {
                return m_actionHistory.Health;
            }

            public void Start()
            {
                if (!IsStarted)
                {
                    IsStarted = true;
                    m_movementValidator.SkipTo(m_manager.m_time.SimulationSteps(), false);
                    m_movementValidator.IsRunning = true;
                }
            }

            public void SendReplication(bool _includeInput, bool _force)
            {
                if (_force || m_lastSendStep < m_movementValidator.Step)
                {
                    Snapshot snapshot = new Snapshot();
                    m_movementHistory.GetSimulation(m_movementValidator.Step.SimulationPeriod(), out SimulationStep? simulation, m_inputHistory);
                    if (simulation != null)
                    {
                        snapshot.simulation = simulation.Value;
                    }
                    m_movementHistory.GetSight(m_movementValidator.Step.SimulationPeriod(), out Sight? sight);
                    if (sight != null)
                    {
                        snapshot.sight = sight.Value;
                    }
                    if (_includeInput)
                    {
                        m_inputHistory.PullReverseInputBuffer(m_movementValidator.Step, m_manager.m_inputStepBuffer, out int count);
                        Serializer.WriteMovementAndInputReplicationMessage(id, m_movementValidator.Step, new ArraySegment<InputStep>(m_manager.m_inputStepBuffer, 0, count), snapshot);
                    }
                    else
                    {
                        Serializer.WriteMovementReplicationMessage(id, m_movementValidator.Step, snapshot);
                    }
                    m_manager.SendAllBut(peer, NetworkManager.SendMethod.Unreliable);
                    m_lastSendStep = m_movementValidator.Step;
                    m_movementHistory.ForgetOlder(m_lastSendStep, true);
                    m_inputHistory.Cut(m_lastSendStep);
                }
            }

            public void Move(int _step, IEnumerable<InputStep> _reversedInputSteps, in Snapshot _snapshot)
            {
                m_movementValidator.Put(_step, _reversedInputSteps, _snapshot.simulation);
                m_movementHistory.Put(_step, _snapshot.sight);
            }

            public void Update()
            {
                // Respawn
                m_actionHistory.Update(m_manager.m_time);
                if (m_actionHistory.ShouldSpawn)
                {
                    double spawnTime = m_manager.m_time + c_spawnDelay;
                    m_actionHistory.PutSpawn(spawnTime);
                    Serializer.WriteSpawnOrderMessage(spawnTime, 0);
                    peer.Send(NetworkManager.SendMethod.ReliableSequenced);
                    Serializer.WriteSpawnReplicationMessage(spawnTime, id, 0);
                    m_manager.SendAllBut(peer, NetworkManager.SendMethod.ReliableSequenced);
                }
                // Components
                m_timeSinceLastCorrection += Time.deltaTime;
                m_movementValidator.UpdateUntil(m_manager.m_time.SimulationSteps());
                // View
                Snapshot snapshot = new Snapshot();
                m_movementHistory.GetSimulation(m_manager.m_time, out SimulationStep? simulation, m_inputHistory);
                if (simulation != null)
                {
                    snapshot.simulation = simulation.Value;
                }
                m_movementHistory.GetSight(m_manager.m_time, out Sight? sight);
                if (sight != null)
                {
                    snapshot.sight = sight.Value;
                }
                m_view.isAlive = m_actionHistory.IsAlive;
                m_view.Move(snapshot);
                m_view.Update(Time.deltaTime);
                // Trim
                m_movementHistory.ForgetOlder((m_manager.m_time - 100).SimulationSteps(), true);
                m_inputHistory.Cut((m_manager.m_time - 100).SimulationSteps());
                m_actionHistory.Trim(m_manager.m_time - 100);
            }

            public void Destroy()
            {
                m_view.Destroy();
            }

            #region MovementValidator.ICorrectionTarget and MovementValidator.IValidationTarget

            void MovementValidator.ICorrectionTarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                if (m_timeSinceLastCorrection >= 1.0f / c_maxCorrectionFrequency)
                {
                    m_timeSinceLastCorrection = 0.0f;
                    Serializer.WriteSimulationCorrectionMessage(_step, _simulation);
                    peer.Send(NetworkManager.SendMethod.Unreliable);
                }
            }

            void MovementValidator.ICorrectionTarget.Rejected(int _step, bool _newer)
            {
            }

            void MovementValidator.IValidationTarget.Validated(int _step, in InputStep _input, in SimulationStep _simulation)
            {
                m_inputHistory.Put(_step, _input);
                m_movementHistory.Put(_step, _simulation);
            }

            #endregion
        }

    }

}
