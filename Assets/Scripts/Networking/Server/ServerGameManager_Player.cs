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
        private abstract class Player : PlayerBase
        {
            protected readonly ServerGameManager m_manager;
            protected readonly MovementHistory m_movementHistory;
            private double m_historyDuration;
            private int? m_lastReplicatedMovementStep;
            private int m_maxMovementInputStepsSendCount;
            private double m_spawnDelay;

            protected Player(ServerGameManager _manager, byte _id) : base(_id, _manager.m_shootStage)
            {
                m_manager = _manager;
                m_movementHistory = new MovementHistory();
                m_lastReplicatedMovementStep = null;
            }

            public double HistoryDuration { get => m_historyDuration; set { Debug.Assert(value >= 0.0); m_historyDuration = value; } }
            public int MaxMovementInputStepsReplicationCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }

            public PlayerRecapInfo RecapInfo => new PlayerRecapInfo
            {
                deaths = m_actionHistory.Deaths,
                kills = m_actionHistory.Kills,
                health = (byte) m_actionHistory.Health,
                id = Id,
                ping = (byte) Mathf.Clamp(Ping, 0, 255)
            };

            public double SpawnDelay { get => m_spawnDelay; set { Debug.Assert(value >= 0.0); m_spawnDelay = value; } }

            public override void Quit(double _time)
            {
                base.Quit(_time);
                Serializer.WriteQuitReplication(_time, Id);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            public void Replicate()
            {
                int lastMovementStep = m_manager.m_time.SimulationSteps();
                if (lastMovementStep <= m_lastReplicatedMovementStep == false)
                {
                    m_lastReplicatedMovementStep = lastMovementStep;
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
                    int maxStepsCount = MaxMovementInputStepsReplicationCount;
                    if (m_lastReplicatedMovementStep != null)
                    {
                        maxStepsCount = Math.Min(maxStepsCount, lastMovementStep - m_lastReplicatedMovementStep.Value);
                    }
                    IEnumerable<InputStep> inputSequence = m_inputHistory.GetReversedInputSequence(lastMovementStep, maxStepsCount);
                    Serializer.WriteMovementAndInputReplication(Id, lastMovementStep, inputSequence, snapshot);
                    SendReplication();
                }
            }

            protected void HandleRespawn()
            {
                if (m_actionHistory.ShouldSpawn)
                {
                    double spawnTime = m_manager.m_time + m_spawnDelay;
                    m_actionHistory.PutSpawn(spawnTime, new SpawnInfo { spawnPoint = 0 });
                    Serializer.WriteSpawnOrderOrReplication(spawnTime, Id, new SpawnInfo());
                    m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
                }
            }

            protected override void OnDeath(double _time, DeathInfo _info)
            {
                Serializer.WriteDeathOrderOrReplication(_time, Id, _info, (byte) m_actionHistory.Deaths);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnShoot(double _time, ShotInfo _info)
            {
                Serializer.WriteShootReplication(_time, Id, _info);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected void PutSight(int _step, Sight _sight)
            {
                m_movementHistory.Put(_step, _sight);
            }

            protected void PutSimulation(int _step, SimulationStep _simulation)
            {
                m_movementHistory.Put(_step, _simulation);
            }

            protected abstract void SendReplication();

            protected void Trim()
            {
                double trimTime = m_manager.m_time - m_historyDuration;
                m_movementHistory.ForgetOlder(trimTime.SimulationSteps(), true);
                Trim(trimTime);
            }

            protected void UpdateView()
            {
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
                UpdateView(snapshot);
            }
        }
    }
}