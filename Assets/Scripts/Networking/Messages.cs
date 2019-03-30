using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking
{

    // Notify: Client tells Server
    // Replication: Server tells Client about someone else
    // Order: Server tells Client about itself
    // Sync: Server tells Client about the room
    internal enum Message
    {
        // Movement
        MovementNotify, SimulationOrder, MovementReplication, MovementAndInputReplication,
        // Room
        TimeSync, ReadyNotify, PlayerIntroductionSync, PlayerWelcomeSync, RecapSync,
        // Actions
        SpawnOrder, DeathOrder, SpawnReplication, DeathReplication, ShootNotify, ShootReplication, HitOrder, HitReplication, KazeNotify
    }

    internal struct PlayerRecapInfo
    {
        public byte id;
        public int kills;
        public int deaths;
        public byte health;
        public byte ping;
    }

    internal static class Serializer
    {

        public static readonly NetDataWriter writer = new NetDataWriter(true, 128);

        #region Primitive put extension methods

        private static void Put(this NetDataWriter _netDataWriter, Enum _value)
        {
            _netDataWriter.Put(Convert.ToByte(_value));
        }

        private static void Put(this NetDataWriter _netDataWriter, in Vector3 _value)
        {
            _netDataWriter.Put(_value.x);
            _netDataWriter.Put(_value.y);
            _netDataWriter.Put(_value.z);
        }

        private static void Put(this NetDataWriter _netDataWriter, in InputStep _value)
        {
            _netDataWriter.Put(_value.dash);
            _netDataWriter.Put(_value.jump);
            _netDataWriter.Put(_value.movementX);
            _netDataWriter.Put(_value.movementZ);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SimulationStep _value)
        {
            _netDataWriter.Put(_value.velocity);
            _netDataWriter.Put(_value.position);
        }

        private static void Put(this NetDataWriter _netDataWriter, in Sight _value)
        {
            _netDataWriter.Put(_value.Turn);
            _netDataWriter.Put(_value.LookUp);
        }

        private static void Put(this NetDataWriter _netDataWriter, in Snapshot _value)
        {
            _netDataWriter.Put(_value.simulation);
            _netDataWriter.Put(_value.sight);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SimulationStepInfo _value)
        {
            _netDataWriter.Put(_value.input);
            _netDataWriter.Put(_value.simulation);
        }

        private static void Put(this NetDataWriter _netDataWriter, in PlayerInfo _value)
        {
            _netDataWriter.Put(_value.name);
        }

        private static void Put(this NetDataWriter _netDataWriter, in PlayerRecapInfo _value)
        {
            _netDataWriter.Put(_value.id);
            _netDataWriter.Put((byte) _value.kills);
            _netDataWriter.Put((byte) _value.deaths);
            _netDataWriter.Put(_value.health);
            _netDataWriter.Put(_value.ping);
        }

        #endregion

        #region Movement messages

        public static void WriteMovementNotifyMessage(int _firstStep, IReadOnlyList<InputStep> _steps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementNotify);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put((byte) _steps.Count);
            foreach (InputStep inputStep in _steps)
            {
                writer.Put(inputStep);
            }
        }

        public static void WriteSimulationCorrectionMessage(int _step, in SimulationStepInfo _simulationStepInfo)
        {
            writer.Reset();
            writer.Put(Message.SimulationOrder);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        public static void WriteMovementReplicationMessage(byte _id, int _step, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
        }

        public static void WriteMovementAndInputReplicationMessage(byte _id, int _step, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementAndInputReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
            writer.Put((byte) _inputSteps.Count);
            foreach (InputStep inputStep in _inputSteps)
            {
                writer.Put(inputStep);
            }
        }

        #endregion

        #region Action messages

        public static void WriteSpawnOrderMessage(double _time, byte _spawnPoint)
        {
            writer.Reset();
            writer.Put(Message.SpawnOrder);
            writer.Put(_time);
            writer.Put(_spawnPoint);
        }

        public static void WriteDeathOrderMessage(double _time)
        {
            writer.Reset();
            writer.Put(Message.DeathOrder);
            writer.Put(_time);
        }

        public static void WriteSpawnReplicationMessage(double _time, byte _id, byte _spawnPoint)
        {
            writer.Reset();
            writer.Put(Message.SpawnReplication);
            writer.Put(_time);
            writer.Put(_id);
            writer.Put(_spawnPoint);
        }

        #endregion

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double)];

        public static void WriteTimeSyncMessage(double _time)
        {
            writer.Reset();
            writer.Put(Message.TimeSync);
            writer.Put(_time);
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            writer.Put(~bytes);
        }

        public static void WriteReadyMessage()
        {
            writer.Reset();
            writer.Put(Message.ReadyNotify);
        }

        public static void WritePlayerIntroductionSyncMessage(byte _id, PlayerInfo _info)
        {
            writer.Reset();
            writer.Put(Message.PlayerIntroductionSync);
            writer.Put(_id);
            writer.Put(_info);
        }

        public static void WritePlayerWelcomeSyncMessage(byte _id)
        {
            writer.Reset();
            writer.Put(Message.PlayerWelcomeSync);
            writer.Put(_id);
            // Checksum
            writer.Put((byte) (255 - _id));
            writer.Put(_id);
            writer.Put((byte) (255 - _id));
        }

        public static void WriteRecapSync(double _time, IEnumerable<PlayerRecapInfo> _recaps)
        {
            writer.Reset();
            writer.Put(Message.RecapSync);
            writer.Put(_time);
            int countPosition = writer.Length;
            writer.Put(0);
            int count = 0;
            foreach (PlayerRecapInfo recap in _recaps)
            {
                writer.Put(recap);
                count++;
            }
            writer.Data[countPosition] = (byte) count;
        }

    }

    internal sealed class Deserializer
    {

        #region Logic and Data

        private readonly NetDataReader m_netDataReader;

        public Deserializer(NetDataReader _netDataReader)
        {
            m_netDataReader = _netDataReader;
        }

        public sealed class DeserializationException : Exception { }

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

        #endregion

        #region Primitive read methods

        private bool ReadBool()
        {
            EnsureRead(m_netDataReader.TryGetBool(out bool value));
            return value;
        }

        private float ReadFloat()
        {
            EnsureRead(m_netDataReader.TryGetFloat(out float value));
            return value;
        }

        private double ReadDouble()
        {
            EnsureRead(m_netDataReader.TryGetDouble(out double value));
            return value;
        }

        private ulong ReadULong()
        {
            EnsureRead(m_netDataReader.TryGetULong(out ulong value));
            return value;
        }

        private uint ReadUint()
        {
            EnsureRead(m_netDataReader.TryGetUInt(out uint value));
            return value;
        }

        private int ReadInt()
        {
            EnsureRead(m_netDataReader.TryGetInt(out int value));
            return value;
        }

        private ushort ReadUShort()
        {
            EnsureRead(m_netDataReader.TryGetUShort(out ushort value));
            return value;
        }

        private byte ReadByte()
        {
            EnsureRead(m_netDataReader.TryGetByte(out byte value));
            return value;
        }

        private string ReadString()
        {
            EnsureRead(m_netDataReader.TryGetString(out string value));
            return value;
        }

        private T ReadEnum<T>() where T : Enum
        {
            byte b = ReadByte();
            object value = Enum.ToObject(typeof(T), b);
            EnsureRead(Enum.IsDefined(typeof(T), value));
            return (T) value;
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

        private Sight ReadSight()
        {
            return new Sight
            {
                Turn = ReadFloat(),
                LookUp = ReadFloat(),
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

        public Message ReadMessageType()
        {
            return ReadEnum<Message>();
        }

        #endregion

        #region Movement messages

        public void ReadMovementNotifyMessage(out int _outStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _outSnapshot)
        {
            _outStep = ReadInt();
            _outSnapshot = ReadSnapshot();
            _outInputStepCount = ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = ReadInputStep();
            }
        }

        public void ReadSimulationCorrectionMessage(out int _outStep, out SimulationStepInfo _outSimulation)
        {
            _outStep = ReadInt();
            _outSimulation = ReadSimulationStepInfo();
        }

        public void ReadMovementReplicationMessage(out byte _id, out int _step, out Snapshot _snapshot)
        {
            _id = ReadByte();
            _step = ReadInt();
            _snapshot = ReadSnapshot();
        }

        public void ReadMovementAndInputReplicationMessage(out byte _outId, out int _outStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _snapshot)
        {
            _outId = ReadByte();
            _outStep = ReadInt();
            _snapshot = ReadSnapshot();
            _outInputStepCount = ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = ReadInputStep();
            }
        }

        #endregion

        #region Action messages

        public void ReadSpawnOrderMessage(out double _outTime, out byte _outSpawnPoint)
        {
            _outTime = ReadDouble();
            _outSpawnPoint = ReadByte();
        }

        public void ReadDeathOrderMessage(out double _outTime)
        {
            _outTime = ReadDouble();
        }

        public void ReadSpawnReplicationMessage(out double _outTime, out byte _outId, out byte _outSpawnPoint)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnPoint = ReadByte();
            EnsureReadEnd();
        }

        #endregion

        #region Room messages

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double) + sizeof(ushort)];

        public void ReadTimeSyncMessage(out double _time)
        {
            _time = ReadDouble();
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            EnsureRead(bytes == ~ReadULong());
            EnsureReadEnd();
        }

        public void ReadPlayerSyncMessage(out double _outTime, out IEnumerable<PlayerRecapInfo> _outInfos)
        {
            _outTime = ReadDouble();
            int count = ReadByte();
            IEnumerable<PlayerRecapInfo> GetInfos()
            {
                while (count-- > 0)
                {
                    yield return ReadPlayerRecapInfo();
                }
            };
            _outInfos = GetInfos();
        }


        public void ReadPlayerWelcomeSyncMessage(out byte _outId)
        {
            _outId = ReadByte();
            // Checksum
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureRead(ReadByte() == _outId);
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureReadEnd();
        }

        public void ReadPlayerIntroductionMessage(out byte _outId, out PlayerInfo _outInfo)
        {
            _outId = ReadByte();
            _outInfo = ReadPlayerInfo();
        }

        public void ReadRecapSync(out double _outTime, out IEnumerable<PlayerRecapInfo> _outInfos)
        {
            _outTime = ReadDouble();
            IEnumerable<PlayerRecapInfo> GetInfos()
            {
                int count = ReadByte();
                while (count-- > 0)
                {
                    yield return ReadPlayerRecapInfo();
                }
            }
            _outInfos = GetInfos();
        }

        #endregion

    }

}
