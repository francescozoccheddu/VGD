﻿using LiteNetLib.Utils;

using System;
using System.Collections.Generic;

using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking
{
    internal sealed class Deserializer
    {
        #region Logic and Data

        private readonly NetDataReader m_netDataReader;

        public Deserializer(NetDataReader _netDataReader)
        {
            m_netDataReader = _netDataReader;
        }

        private static void EnsureRead(bool _read)
        {
            if (!_read)
            {
                throw new DeserializationException();
            }
        }

        private void EnsureReadEnd()
        {
            EnsureRead(m_netDataReader.EndOfData);
        }

        public sealed class DeserializationException : Exception { }

        #endregion Logic and Data

        #region Primitive read methods

        public Message ReadMessageType()
        {
            return ReadEnum<Message>();
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

        private DamageInfo ReadDamageInfo()
        {
            return new DamageInfo
            {
                offensePosition = ReadVector3(),
                offenseType = ReadEnum<OffenseType>()
            };
        }

        private DeathInfo ReadDeathInfo()
        {
            return new DeathInfo
            {
                killerId = ReadByte(),
                offenseType = ReadEnum<OffenseType>(),
                isExploded = ReadBool()
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

        private HitConfirmInfo ReadHitConfirmInfo()
        {
            return new HitConfirmInfo
            {
                offenseType = ReadEnum<OffenseType>()
            };
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

        private PlayerInfo ReadPlayerInfo()
        {
            return new PlayerInfo
            {
                name = ReadString(),
            };
        }

        private PlayerRecapInfo ReadPlayerRecapInfo()
        {
            return new PlayerRecapInfo
            {
                id = ReadByte(),
                kills = ReadByte(),
                deaths = ReadByte(),
                health = ReadByte(),
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

        private SimulationStep ReadSimulationStep()
        {
            return new SimulationStep
            {
                velocity = ReadVector3(),
                position = ReadVector3()
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
                spawnPoint = ReadInt()
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

        #endregion Primitive read methods

        #region Movement messages

        public void ReadMovementNotify(out int _outStep, out IEnumerable<InputStep> _outInputSteps, out Snapshot _outSnapshot)
        {
            _outStep = ReadInt();
            _outSnapshot = ReadSnapshot();
            _outInputSteps = ReadArray(ReadInputStep);
        }

        public void ReadMovementReplication(out byte _outId, out int _outStep, out IEnumerable<InputStep> _outInputSteps, out Snapshot _snapshot)
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
        }

        #endregion Movement messages

        #region Action messages

        public void ReadDamageOrder(out double _outTime, out DamageInfo _outInfo, out byte _outHealth)
        {
            _outTime = ReadDouble();
            _outInfo = ReadDamageInfo();
            _outHealth = ReadByte();
        }

        public void ReadDeathOrder(out double _outTime)
        {
            _outTime = ReadDouble();
        }

        public void ReadDeathOrderOrReplication(out double _outTime, out byte _outId, out DeathInfo _outDeathInfo, out byte _outDeaths)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outDeathInfo = ReadDeathInfo();
            _outDeaths = ReadByte();
        }

        public void ReadHitConfirmOrder(out double _outTime, out HitConfirmInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outInfo = ReadHitConfirmInfo();
        }

        public void ReadKazeNotify(out double _outTime)
        {
            _outTime = ReadDouble();
        }

        public void ReadShotNotify(out double _outTime, out ShotInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outInfo = ReadShotInfo();
        }

        public void ReadShotReplication(out double _outTime, out byte _outId, out ShotInfo _outInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outInfo = ReadShotInfo();
        }

        public void ReadSpawnOrderOrReplication(out double _outTime, out byte _outId, out SpawnInfo _outSpawnInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnInfo = ReadSpawnInfo();
        }

        #endregion Action messages

        #region Room messages

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double) + sizeof(ushort)];

        public void ReadPlayerIntroduction(out byte _outId, out PlayerInfo _outInfo)
        {
            _outId = ReadByte();
            _outInfo = ReadPlayerInfo();
        }

        public void ReadPlayerSync(out double _outTime, out IEnumerable<PlayerRecapInfo> _outInfos)
        {
            _outTime = ReadDouble();
            _outInfos = ReadArray(ReadPlayerRecapInfo);
        }

        public void ReadPlayerWelcomeSync(out byte _outId)
        {
            _outId = ReadByte();
            // Checksum
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureRead(ReadByte() == _outId);
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureReadEnd();
        }

        public void ReadQuitReplication(out double _outTime, out byte _outId)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
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

        #endregion Room messages
    }
}