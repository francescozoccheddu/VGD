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
            private int? m_lastReplicatedMovementStep;
            private int m_maxMovementInputStepsSendCount;

            protected Player(ServerGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_manager = _manager;
                m_ShouldHandleRespawn = true;
            }

            public int MaxMovementInputStepsReplicationCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }

            public PlayerRecapInfo RecapInfo => new PlayerRecapInfo
            {
                deaths = State.Deaths,
                kills = State.Kills,
                health = (byte) State.Health,
                id = Id,
                ping = (byte) Mathf.Clamp(Ping, 0, 255)
            };

            public void Replicate()
            {
                int lastMovementStep = m_manager.m_time.SimulationSteps();
                if (lastMovementStep <= m_lastReplicatedMovementStep == false)
                {
                    m_lastReplicatedMovementStep = lastMovementStep;
                    int maxStepsCount = MaxMovementInputStepsReplicationCount;
                    if (m_lastReplicatedMovementStep != null)
                    {
                        maxStepsCount = Math.Min(maxStepsCount, lastMovementStep - m_lastReplicatedMovementStep.Value);
                    }
                    IEnumerable<InputStep> inputSequence = GetReversedInputSequence(lastMovementStep, maxStepsCount);
                    Serializer.WriteMovementAndInputReplication(Id, lastMovementStep, inputSequence, Snapshot);
                }
            }

            protected override void OnDeathScheduled(double _time, DeathInfo _info)
            {
                Serializer.WriteDeathOrderOrReplication(_time, Id, _info, (byte) State.Deaths);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnQuitScheduled(double _time)
            {
                Serializer.WriteQuitReplication(_time, Id);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnShootScheduled(double _time, ShotInfo _info)
            {
                Serializer.WriteShootReplication(_time, Id, _info);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnSpawnScheduled(double _time, SpawnInfo _info)
            {
                Serializer.WriteSpawnOrderOrReplication(_time, Id, _info);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected abstract void SendReplication(NetworkManager.SendMethod _method);
        }
    }
}