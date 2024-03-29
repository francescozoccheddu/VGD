﻿using LiteNetLib.Utils;

using System;
using System.Collections.Generic;

using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;

namespace Wheeled.Networking
{
    public sealed class Deserializer
    {
        public sealed class DeserializationException : Exception { }

        private readonly NetDataReader m_netDataReader;

        public Deserializer(NetDataReader _netDataReader)
        {
            m_netDataReader = _netDataReader;
        }

        public EMessage ReadMessageType()
        {
            return ReadEnum<EMessage>();
        }

        public void ReadMovementNotify(out int _outStep, out IEnumerable<InputStep> _outInputSteps, out Snapshot _outSnapshot)
        {
            _outStep = ReadInt();
            _outSnapshot = ReadSnapshot();
            _outInputSteps = ReadArray(ReadInputStep);
        }

        public void ReadMovementReplication(out int _outId, out int _outStep, out IEnumerable<InputStep> _outInputSteps, out Snapshot _snapshot)
        {
            _outId = ReadByte();
            _outStep = ReadInt();
            _snapshot = ReadSnapshot();
            _outInputSteps = ReadArray(ReadInputStep);
        }

        public void ReadSimulationOrder(out int _outStep, out SimulationStepInfo _outSimulation)
        {
            _outStep = ReadInt();
            _outSimulation = ReadSimulationStepInfo();
            EnsureReadEnd();
        }

        public void ReadDamageOrderOrReplication(out double _outTime, out int _outId, out DamageInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outInfo = ReadDamageInfo();
            EnsureReadEnd();
        }

        public void ReadKazeNotify(out double _outTime, out KazeInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outInfo = ReadKazeInfo();
            EnsureReadEnd();
        }

        public void ReadShotNotify(out double _outTime, out ShotInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outInfo = ReadShotInfo();
            EnsureReadEnd();
        }

        public void ReadShotReplication(out double _outTime, out int _outId, out ShotInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outInfo = ReadShotInfo();
            EnsureReadEnd();
        }

        public void ReadSpawnOrderOrReplication(out double _outTime, out int _outId, out SpawnInfo _outSpawnInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnInfo = ReadSpawnInfo();
            EnsureReadEnd();
        }

        public void ReadPlayerIntroduction(out int _outId, out PlayerInfo _outInfo)
        {
            _outId = ReadByte();
            _outInfo = ReadPlayerInfo();
            EnsureReadEnd();
        }

        public void ReadPlayerSync(out double _outTime, out IEnumerable<PlayerRecapInfo> _outInfos)
        {
            _outTime = ReadDouble();
            _outInfos = ReadArray(ReadPlayerRecapInfo);
        }

        public void ReadPlayerWelcomeSync(out int _outId, out int _outMap)
        {
            _outId = ReadByte();
            _outMap = ReadByte();
            EnsureRead(_outMap >= 0 && _outMap < Scripts.Scenes.arenas.Length);
            // Checksum
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureRead(ReadByte() == _outId);
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureReadEnd();
        }

        public void ReadQuitReplication(out double _outTime, out int _outId)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            EnsureReadEnd();
        }

        public void ReadRecapSync(out double _outTime, out IEnumerable<PlayerRecapInfo> _outInfos)
        {
            _outTime = ReadDouble();
            _outInfos = ReadArray(ReadPlayerRecapInfo);
        }

        public void ReadTimeSync(out double _time)
        {
            _time = ReadDouble();
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            EnsureRead(bytes == ~ReadULong());
            EnsureReadEnd();
        }

        public void ReadKillSync(out double _outTime, out KillInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outInfo = ReadKillInfo();
            EnsureReadEnd();
        }

        public void ReadDiscoveryInfo(out int _outArena)
        {
            _outArena = ReadByte();
            EnsureRead(_outArena >= 0 && _outArena < Scripts.Scenes.arenas.Length);
            EnsureReadEnd();
        }

        public PlayerInfo ReadPlayerInfo()
        {
            var info = new PlayerInfo
            {
                name = ReadString(),
                color = ReadByte(),
                head = ReadByte()
            };
            EnsureRead(info.color >= 0 && info.color < Scripts.PlayerPreferences.colors.Length);
            EnsureRead(info.head >= 0 && info.head < Scripts.PlayerPreferences.heads.Length);
            return info;
        }

        private static void EnsureRead(bool _read)
        {
            if (!_read)
            {
                throw new DeserializationException();
            }
        }

        public void EnsureReadEnd()
        {
            EnsureRead(m_netDataReader.EndOfData);
        }

        private IEnumerable<T> ReadArray<T>(Func<T> _read)
        {
            int count = ReadByte();
            while (count-- > 0)
            {
                yield return _read();
            }
        }

        private bool ReadBool()
        {
            EnsureRead(m_netDataReader.TryGetBool(out bool value));
            return value;
        }

        private byte ReadByte()
        {
            EnsureRead(m_netDataReader.TryGetByte(out byte value));
            return value;
        }

        private int ReadHealth()
        {
            return ReadByte() + LifeHistory.c_explosionHealth;
        }

        private DamageInfo ReadDamageInfo()
        {
            return new DamageInfo
            {
                damage = ReadHealth(),
                maxHealth = ReadHealth(),
                offenderId = ReadByte(),
                offenseType = ReadEnum<EOffenseType>()
            };
        }

        private double ReadDouble()
        {
            EnsureRead(m_netDataReader.TryGetDouble(out double value));
            return value;
        }

        private T ReadEnum<T>() where T : Enum
        {
            byte b = ReadByte();
            object value = Enum.ToObject(typeof(T), b);
            EnsureRead(Enum.IsDefined(typeof(T), value));
            return (T) value;
        }

        private float ReadFloat()
        {
            EnsureRead(m_netDataReader.TryGetFloat(out float value));
            return value;
        }

        private InputStep ReadInputStep()
        {
            return new InputStep
            {
                dash = ReadBool(),
                jump = ReadBool(),
                movementX = ReadFloat(),
                movementZ = ReadFloat()
            };
        }

        private int ReadInt()
        {
            EnsureRead(m_netDataReader.TryGetInt(out int value));
            return value;
        }

        private PlayerRecapInfo ReadPlayerRecapInfo()
        {
            return new PlayerRecapInfo
            {
                id = ReadByte(),
                kills = ReadByte(),
                deaths = ReadByte(),
                health = ReadHealth(),
                ping = ReadByte(),
            };
        }

        private ShotInfo ReadShotInfo()
        {
            return new ShotInfo
            {
                position = ReadVector3(),
                sight = ReadSight(),
                isRocket = ReadBool()
            };
        }

        private Sight ReadSight()
        {
            return new Sight
            {
                Turn = ReadFloat(),
                LookUp = ReadFloat(),
            };
        }

        private CharacterController ReadSimulationStep()
        {
            return new CharacterController
            {
                Velocity = ReadVector3(),
                Position = ReadVector3(),
                dashStamina = ReadFloat()
            };
        }

        private SimulationStepInfo ReadSimulationStepInfo()
        {
            return new SimulationStepInfo
            {
                input = ReadInputStep(),
                simulation = ReadSimulationStep()
            };
        }

        private Snapshot ReadSnapshot()
        {
            return new Snapshot
            {
                simulation = ReadSimulationStep(),
                sight = ReadSight()
            };
        }

        private SpawnInfo ReadSpawnInfo()
        {
            return new SpawnInfo
            {
                spawnPoint = ReadByte()
            };
        }

        private KazeInfo ReadKazeInfo()
        {
            return new KazeInfo
            {
                position = ReadVector3()
            };
        }

        private string ReadString()
        {
            EnsureRead(m_netDataReader.TryGetString(out string value));
            return value;
        }

        private uint ReadUint()
        {
            EnsureRead(m_netDataReader.TryGetUInt(out uint value));
            return value;
        }

        private ulong ReadULong()
        {
            EnsureRead(m_netDataReader.TryGetULong(out ulong value));
            return value;
        }

        private ushort ReadUShort()
        {
            EnsureRead(m_netDataReader.TryGetUShort(out ushort value));
            return value;
        }

        private Vector3 ReadVector3()
        {
            return new Vector3
            {
                x = ReadFloat(),
                y = ReadFloat(),
                z = ReadFloat()
            };
        }

        private KillInfo ReadKillInfo()
        {
            return new KillInfo()
            {
                killerId = ReadByte(),
                victimId = ReadByte(),
                offenseType = ReadEnum<EOffenseType>(),
                killerKills = ReadByte(),
                victimDeaths = ReadByte()
            };
        }
    }
}