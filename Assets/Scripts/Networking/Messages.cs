using LiteNetLib.Utils;

using System;
using System.Collections.Generic;

using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
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
        MovementNotify, SimulationOrder, MovementReplication,

        // Room
        TimeSync, ReadyNotify, PlayerIntroductionSync, PlayerWelcomeSync, RecapSync, QuitReplication,

        // Actions
        KazeNotify, GunShotNotify, DeathOrderOrReplication, SpawnOrderOrReplication, GunShotReplication, GunHitOrderOrReplication, HitConfirmOrder
    }

    internal struct PlayerRecapInfo
    {
        public int deaths;
        public byte health;
        public byte id;
        public int kills;
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

        private static void Put(this NetDataWriter _netDataWriter, in KazeInfo _value)
        {
            _netDataWriter.Put(_value.position);
        }

        private static void Put(this NetDataWriter _netDataWriter, in DeathInfo _value)
        {
            _netDataWriter.Put(_value.deadId);
            _netDataWriter.Put(_value.killerId);
            _netDataWriter.Put(_value.cause);
            _netDataWriter.Put(_value.position);
            _netDataWriter.Put(_value.explosion);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SpawnInfo _value)
        {
            _netDataWriter.Put(_value.spawnPoint);
        }

        private static void Put(this NetDataWriter _netDataWriter, in GunShotInfo _value)
        {
            _netDataWriter.Put(_value.id);
            _netDataWriter.Put(_value.position);
            _netDataWriter.Put(_value.sight);
            _netDataWriter.Put(_value.rocket);
            _netDataWriter.Put(_value.power);
        }

        private static void Put(this NetDataWriter _netDataWriter, in GunHitInfo _value)
        {
            _netDataWriter.Put(_value.id);
            _netDataWriter.Put(_value.position);
            _netDataWriter.Put(_value.normal);
        }

        private static void Put<T>(this NetDataWriter _netDataWriter, IEnumerable<T> _value, Action<NetDataWriter, T> _put)
        {
            int countPosition = _netDataWriter.Length;
            _netDataWriter.Put((byte) 0);
            int count = 0;
            foreach (T item in _value)
            {
                count++;
                _put(_netDataWriter, item);
                if (count >= 255)
                {
                    break;
                }
            }
            writer.Data[countPosition] = (byte) count;
        }

        #endregion Primitive put extension methods

        #region Movement messages

        public static void WriteMovementAndInputReplication(byte _id, int _step, IEnumerable<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
            writer.Put(_inputSteps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteMovementNotify(int _firstStep, IEnumerable<InputStep> _steps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementNotify);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put(_steps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteSimulationCorrection(int _step, in SimulationStepInfo _simulationStepInfo)
        {
            writer.Reset();
            writer.Put(Message.SimulationOrder);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        #endregion Movement messages

        #region Action messages

        public static void WriteDeathOrderOrReplication(double _time, DeathInfo _info)
        {
            writer.Reset();
            writer.Put(Message.DeathOrderOrReplication);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteGunHitOrderOrReplication(double _time, bool _isOrder, GunHitInfo _info)
        {
            writer.Reset();
            writer.Put(Message.GunHitOrderOrReplication);
            writer.Put(_time);
            writer.Put(_isOrder);
            writer.Put(_info);
        }

        public static void WriteGunShotNotify(double _time, Vector3 _position, Sight _sight, bool _isRocket)
        {
            writer.Reset();
            writer.Put(Message.GunShotNotify);
            writer.Put(_time);
            writer.Put(_position);
            writer.Put(_sight);
            writer.Put(_isRocket);
        }

        public static void WriteGunShotReplication(double _time, GunShotInfo _info)
        {
            writer.Reset();
            writer.Put(Message.GunShotReplication);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteHitConfirmOrder(double _time)
        {
            writer.Reset();
            writer.Put(Message.HitConfirmOrder);
            writer.Put(_time);
        }

        public static void WriteKazeNotify(double _time, KazeInfo _info)
        {
            writer.Reset();
            writer.Put(Message.KazeNotify);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteSpawnOrderOrReplication(double _time, byte _id, SpawnInfo _info)
        {
            writer.Reset();
            writer.Put(Message.SpawnOrderOrReplication);
            writer.Put(_time);
            writer.Put(_id);
            writer.Put(_info);
        }

        #endregion Action messages

        #region Room messages

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double)];

        public static void WritePlayerIntroductionSync(byte _id, PlayerInfo _info)
        {
            writer.Reset();
            writer.Put(Message.PlayerIntroductionSync);
            writer.Put(_id);
            writer.Put(_info);
        }

        public static void WritePlayerWelcomeSync(byte _id)
        {
            writer.Reset();
            writer.Put(Message.PlayerWelcomeSync);
            writer.Put(_id);
            // Checksum
            writer.Put((byte) (255 - _id));
            writer.Put(_id);
            writer.Put((byte) (255 - _id));
        }

        public static void WriteQuitReplication(double _time, byte _id)
        {
            writer.Reset();
            writer.Put(Message.QuitReplication);
            writer.Put(_time);
            writer.Put(_id);
        }

        public static void WriteReady()
        {
            writer.Reset();
            writer.Put(Message.ReadyNotify);
        }

        public static void WriteRecapSync(double _time, IEnumerable<PlayerRecapInfo> _recaps)
        {
            writer.Reset();
            writer.Put(Message.RecapSync);
            writer.Put(_time);
            writer.Put(_recaps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteTimeSync(double _time)
        {
            writer.Reset();
            writer.Put(Message.TimeSync);
            writer.Put(_time);
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            writer.Put(~bytes);
        }

        #endregion Room messages
    }

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

        private DeathInfo ReadDeathInfo()
        {
            return new DeathInfo
            {
                deadId = ReadByte(),
                killerId = ReadByte(),
                cause = ReadEnum<DeathCause>(),
                position = ReadVector3(),
                explosion = ReadBool()
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

        private GunHitInfo ReadGunHitInfo()
        {
            return new GunHitInfo
            {
                id = ReadByte(),
                position = ReadVector3(),
                normal = ReadVector3()
            };
        }

        private GunShotInfo ReadGunShotInfo()
        {
            return new GunShotInfo
            {
                id = ReadByte(),
                position = ReadVector3(),
                sight = ReadSight(),
                rocket = ReadBool(),
                power = ReadFloat()
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

        private KazeInfo ReadKazeInfo()
        {
            return new KazeInfo
            {
                position = ReadVector3()
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

        public void ReadMovementReplication(out byte _outId, out int _outStep, out IEnumerable<InputStep> _outInputSteps, out Snapshot _snapshot)
        {
            _outId = ReadByte();
            _outStep = ReadInt();
            _snapshot = ReadSnapshot();
            _outInputSteps = ReadArray(ReadInputStep);
        }

        public void ReadMovementNotify(out int _outStep, out IEnumerable<InputStep> _outInputSteps, out Snapshot _outSnapshot)
        {
            _outStep = ReadInt();
            _outSnapshot = ReadSnapshot();
            _outInputSteps = ReadArray(ReadInputStep);
        }

        public void ReadSimulationCorrection(out int _outStep, out SimulationStepInfo _outSimulation)
        {
            _outStep = ReadInt();
            _outSimulation = ReadSimulationStepInfo();
        }

        #endregion Movement messages

        #region Action messages

        public void ReadDeathOrder(out double _outTime)
        {
            _outTime = ReadDouble();
        }

        public void ReadDeathOrderOrReplication(out double _outTime, out DeathInfo _outDeathInfo)
        {
            _outTime = ReadDouble();
            _outDeathInfo = ReadDeathInfo();
        }

        public void ReadGunHitOrderOrReplication(out double _outTime, out bool _outIsOrder, out GunHitInfo _outGunHitInfo)
        {
            _outTime = ReadDouble();
            _outIsOrder = ReadBool();
            _outGunHitInfo = ReadGunHitInfo();
        }

        public void ReadGunShotNotify(out double _outTime, out Vector3 _outPosition, out Sight _outSight, out bool _outIsRocket)
        {
            _outTime = ReadDouble();
            _outPosition = ReadVector3();
            _outSight = ReadSight();
            _outIsRocket = ReadBool();
        }

        public void ReadGunShotReplication(out double _outTime, out GunShotInfo _outGunShotInfo)
        {
            _outTime = ReadDouble();
            _outGunShotInfo = ReadGunShotInfo();
        }

        public void ReadHitConfirmOrder(out double _outTime)
        {
            _outTime = ReadDouble();
        }

        public void ReadKazeNotify(out double _outTime, out KazeInfo _outKazeInfo)
        {
            _outTime = ReadDouble();
            _outKazeInfo = ReadKazeInfo();
        }

        public void ReadSpawnOrder(out double _outTime, out byte _outSpawnPoint)
        {
            _outTime = ReadDouble();
            _outSpawnPoint = ReadByte();
        }

        public void ReadSpawnOrderOrReplication(out double _outTime, out byte _outId, out SpawnInfo _outSpawnInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnInfo = ReadSpawnInfo();
        }

        public void ReadSpawnReplication(out double _outTime, out byte _outId, out byte _outSpawnPoint)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnPoint = ReadByte();
            EnsureReadEnd();
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