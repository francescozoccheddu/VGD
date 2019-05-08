using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Scene;
using Wheeled.Gameplay.Stage;
using Wheeled.HUD;

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

            private const double c_spawnDelay = 0.5;
            private const double c_respawnWaitTime = 3.0;

            private double m_nextSpawnTime;
            private double m_lastValidatedDeathTime;
            private double m_lastValidatedExplosionTime;
            private int? m_lastReplicatedMovementStep;
            private int m_maxMovementInputStepsSendCount;
            private double m_damageValidationDelay;

            #endregion Private Fields

            #region Protected Constructors

            protected AuthoritativePlayer(ServerGameManager _manager, byte _id, OffenseBackstage _offenseBackstage) : base(_manager, _id, _offenseBackstage)
            {
                m_manager = _manager;
                m_lastValidatedDeathTime = double.NegativeInfinity;
                m_lastValidatedExplosionTime = double.NegativeInfinity;
                m_nextSpawnTime = double.NaN;
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
                    m_manager.UpdateScoreBoard();
                }
            }

            public byte? GetExplosionOffenderIdRecursive(double _time)
            {
                LifeHistory.GetLastDeathInfo(_time, out _, out DamageNode? node);
                if (node != null)
                {
                    DamageInfo info = node.Value.damage;
                    if (info.offenderId == Id || info.offenseType != OffenseType.Explosion)
                    {
                        return info.offenderId;
                    }
                    else
                    {
                        return m_manager.GetPlayerById(info.offenderId)?.GetExplosionOffenderIdRecursive(node.Value.time);
                    }
                }
                return null;
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
                if (_info.offenderId == m_manager.m_localPlayer.Id && _info.offenderId != Id)
                {
                    m_manager.m_localPlayer.PutHitConfirm(_time, _info.offenseType);
                }
            }

            protected override void OnQuitScheduled(double _time)
            {
                Serializer.WriteQuitReplication(_time, Id);
                m_manager.SendAll(NetworkManager.SendMethod.ReliableUnordered);
                m_manager.m_players.Remove(this);
                m_manager.MatchBoard.Put(_time, new MatchBoard.QuitEvent
                {
                    player = this
                });
                Destroy();
                m_manager.UpdateScoreBoard();
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
                LifeHistory.GetLastDeathInfo(m_manager.m_time, out DamageNode? death, out DamageNode? explosion);
                if (death != null)
                {
                    ValidateKill(death.Value);
                }
                if (explosion != null)
                {
                    ValidateExplosion(explosion.Value);
                }
            }

            private void ValidateKill(DamageNode _node)
            {
                if (_node.time > m_lastValidatedDeathTime && _node.time <= m_manager.m_time - m_damageValidationDelay)
                {
                    m_nextSpawnTime = m_manager.m_time + c_respawnWaitTime;
                    m_lastValidatedDeathTime = _node.time;
                    DeathsValue.Put(_node.time, Deaths + 1);
                    AuthoritativePlayer killer = null;
                    byte offenderId = _node.damage.offenderId;
                    if (offenderId != Id)
                    {
                        if (_node.damage.offenseType == OffenseType.Explosion)
                        {
                            offenderId = m_manager.GetPlayerById(offenderId)
                                ?.GetExplosionOffenderIdRecursive(_node.time) ?? offenderId;
                        }
                        killer = m_manager.GetPlayerById(offenderId);
                        killer?.KillsValue.Put(_node.time, killer.Kills + 1);
                    }
                    Serializer.WriteKillSync(_node.time, new KillInfo
                    {
                        killerId = offenderId,
                        offenseType = _node.damage.offenseType,
                        victimId = Id,
                        victimDeaths = (byte) Deaths,
                        killerKills = (byte) (killer?.Kills ?? 0)
                    });
                    m_manager.MatchBoard.Put(m_manager.m_time, new MatchBoard.KillEvent
                    {
                        killer = killer,
                        victim = this,
                        offenseType = _node.damage.offenseType
                    });
                    m_manager.SendAll(NetworkManager.SendMethod.ReliableSequenced);
                    m_manager.UpdateScoreBoard();
                }
            }

            private void ValidateExplosion(DamageNode _node)
            {
                if (_node.time > m_lastValidatedExplosionTime)
                {
                    m_lastValidatedExplosionTime = _node.time;
                    m_manager.m_offenseBackstage.PutExplosion(_node.time, new ExplosionOffense(Id, this.GetSnapshot(_node.time).simulation.Position));
                }
            }

            private void Spawn()
            {
                m_nextSpawnTime = double.NaN;
                double spawnTime = m_manager.m_time + c_spawnDelay;
                IEnumerable<Vector3> positions = from p in m_manager.m_players
                                                 where !p.IsQuit(spawnTime)
                                                 && p.LifeHistory.IsAlive(spawnTime)
                                                 select p.GetSnapshot(spawnTime).simulation.Position;
                PutSpawn(spawnTime, new SpawnInfo()
                {
                    spawnPoint = SpawnManagerBehaviour.Spawn(positions)
                });
            }

            private void HandleRespawn()
            {
                if (m_manager.m_time > m_nextSpawnTime)
                {
                    Spawn();
                }
            }

            #endregion Private Methods
        }

        #endregion Private Classes
    }
}