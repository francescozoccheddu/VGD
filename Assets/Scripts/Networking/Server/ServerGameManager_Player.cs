using System;
using System.Collections.Generic;

using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        #region Private Classes

        private abstract class AuthoritativePlayer : Player
        {
            #region Public Properties

            public int MaxMovementInputStepsReplicationCount { get => m_maxMovementInputStepsSendCount; set { Debug.Assert(value >= 0); m_maxMovementInputStepsSendCount = value; } }
            public double DamageValidationDelay { get => m_damageValidationDelay; set { Debug.Assert(value >= 0); m_damageValidationDelay = value; } }
            public bool IsStarted { get; private set; }

            #endregion Public Properties

            #region Protected Fields

            protected readonly ServerGameManager m_manager;

            #endregion Protected Fields

            #region Private Fields

            private const double c_respawnWaitTime = 3.0;

            private double m_lastSpawnTime;
            private double m_lastValidatedDeathTime;
            private double m_lastValidatedExplosionTime;
            private int? m_lastReplicatedMovementStep;
            private int m_maxMovementInputStepsSendCount;
            private double m_damageValidationDelay;

            #endregion Private Fields

            #region Protected Constructors

            protected AuthoritativePlayer(ServerGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_manager = _manager;
                m_lastValidatedDeathTime = double.NegativeInfinity;
                m_lastValidatedExplosionTime = double.NegativeInfinity;
                m_lastSpawnTime = double.NegativeInfinity;
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
                    Spawn();
                }
            }

            #endregion Public Methods

            #region Protected Methods

            protected abstract int GetLastValidMovementStep();

            protected override void OnUpdated()
            {
                ValidateDamage();
                HandleRespawn();
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

            #region Private Methods

            private void ValidateDamage()
            {
                double validationTime = m_manager.m_time - m_damageValidationDelay;
                if ((validationTime > m_lastValidatedDeathTime || validationTime > m_lastValidatedExplosionTime)
                    && !LifeHistory.IsAlive(validationTime))
                {
                    LifeHistory.GetLastDeathInfo(validationTime, out DamageNode? death, out DamageNode? explosion);
                    if (death?.time > m_lastValidatedDeathTime)
                    {
                        DamageInfo damage = death.Value.damage;
                        m_lastValidatedDeathTime = death.Value.time;
                        DeathsValue.Put(validationTime, Deaths + 1);
                        AuthoritativePlayer killer = null;
                        if (damage.offenderId != Id)
                        {
                            killer = m_manager.GetPlayerById(damage.offenderId);
                            killer?.KillsValue.Put(validationTime, killer.Kills + 1);
                        }
                        Serializer.WriteKillSync(death.Value.time, new KillInfo
                        {
                            killerId = damage.offenderId,
                            offenseType = damage.offenseType,
                            victimId = Id,
                            victimDeaths = (byte) Deaths,
                            killerKills = (byte) (killer?.Kills ?? 0)
                        });
                        m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
                    }
                    if (explosion?.time > m_lastValidatedExplosionTime)
                    {
                        double time = explosion.Value.time;
                        byte offenderId = explosion.Value.damage.offenderId;
                        Vector3 position = this.GetSnapshot(time).simulation.Position;
                        m_manager.m_offenseBackstage.PutExplosion(explosion.Value.time, new ExplosionOffense(offenderId, position));
                        m_lastValidatedExplosionTime = time;
                    }
                }
            }

            private void Spawn()
            {
                PutSpawn(m_manager.m_time + c_spawnDelay, new SpawnInfo()
                {
                });
            }

            private void HandleRespawn()
            {
                if (m_manager.m_time > m_lastSpawnTime)
                {
                    double? elapsed = LifeHistory.GetTimeSinceLastDeath(m_manager.m_time);
                    if (elapsed >= c_respawnWaitTime)
                    {
                        m_lastSpawnTime = m_manager.m_time;
                        Spawn();
                    }
                }
            }

            #endregion Private Methods
        }

        #endregion Private Classes

        #region Public Fields

        public const double c_spawnDelay = 0.5;

        #endregion Public Fields
    }
}