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
        #region Private Classes

        private abstract class Player : PlayerBase
        {
            #region Public Properties

            public int MaxMovementInputStepsReplicationCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }

            public bool IsStarted { get; private set; }

            #endregion Public Properties

            #region Protected Fields

            protected readonly ServerGameManager m_manager;

            #endregion Protected Fields

            #region Private Fields

            private int? m_lastReplicatedMovementStep;
            private int m_maxMovementInputStepsSendCount;

            #endregion Private Fields

            #region Protected Constructors

            protected Player(ServerGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_manager = _manager;
            }

            #endregion Protected Constructors

            #region Public Methods

            public PlayerRecapInfo RecapInfo(double _time)
            {
                return new PlayerRecapInfo
                {
                    deaths = Deaths,
                    health = LifeHistory.GetHealth(_time),
                    id = Id,
                    kills = Kills,
                    ping = (byte) Mathf.Clamp(Ping, 0, 255)
                };
            }

            public void Replicate()
            {
                int lastMovementStep = GetLastValidMovementStep();
                if (lastMovementStep <= m_lastReplicatedMovementStep == false)
                {
                    m_lastReplicatedMovementStep = lastMovementStep;
                    int maxStepsCount = MaxMovementInputStepsReplicationCount;
                    if (m_lastReplicatedMovementStep != null)
                    {
                        maxStepsCount = Math.Min(maxStepsCount, lastMovementStep - m_lastReplicatedMovementStep.Value);
                    }
                    IEnumerable<InputStep> inputSequence = InputHistory.GetReversedInputSequence(lastMovementStep, maxStepsCount);
                    Serializer.WriteMovementAndInputReplication(Id, lastMovementStep, inputSequence, this.GetSnapshot(lastMovementStep.SimulationPeriod()));
                    SendReplication(NetworkManager.SendMethod.Unreliable);
                }
            }

            public void Start()
            {
                if (!IsStarted)
                {
                    IsStarted = true;
                    PutSpawn(m_manager.m_time + c_spawnDelay, new SpawnInfo());
                }
            }

            #endregion Public Methods

            #region Protected Methods

            protected abstract int GetLastValidMovementStep();

            protected override void OnUpdated()
            {
                // TODO Look for kills
            }

            protected override void OnDamageScheduled(double _time, DamageInfo _info)
            {
                Serializer.WriteDamageOrderOrReplication(_time, Id, _info);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnQuitScheduled(double _time)
            {
                Serializer.WriteQuitReplication(_time, Id);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
            }

            protected override void OnShotScheduled(double _time, ShotInfo _info)
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

            #endregion Protected Methods
        }

        #endregion Private Classes

        #region Public Fields

        public const double c_spawnDelay = 0.5;

        #endregion Public Fields
    }
}