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
        MovementNotify, SimulationOrder, MovementReplication, MovementAndInputReplication,
        // Room
        TimeSync, ReadyNotify, PlayerIntroductionSync, PlayerWelcomeSync, RecapSync, QuitReplication,
        // Actions
        SpawnOrder, DeathOrder, SpawnReplication, KazeNotify, GunShotNotify, DeathOrderOrReplication, SpawnOrderOrReplication, GunShotReplication, GunHitOrderOrReplication, HitConfirmOrder
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

        #endregion

        #region Movement messages

        public static void WriteMovementNotify(int _firstStep, IReadOnlyList<InputStep> _steps, in Snapshot _snapshot)
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

        public static void WriteSimulationCorrection(int _step, in SimulationStepInfo _simulationStepInfo)
        {
            writer.Reset();
            writer.Put(Message.SimulationOrder);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        public static void WriteMovementReplication(byte _id, int _step, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
        }

        public static void WriteMovementAndInputReplication(byte _id, int _step, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
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
        public static void WriteSpawnOrder(double _time, byte _spawnPoint)
        {
            writer.Reset();
            writer.Put(Message.SpawnOrder);
            writer.Put(_time);
            writer.Put(_spawnPoint);
        }

        public static void WriteDeathOrder(double _time)
        {
            writer.Reset();
            writer.Put(Message.DeathOrder);
            writer.Put(_time);
        }

        public static void WriteSpawnReplication(double _time, byte _id, byte _spawnPoint)
        {
            writer.Reset();
            writer.Put(Message.SpawnReplication);
            writer.Put(_time);
            writer.Put(_id);
            writer.Put(_spawnPoint);
        }

        public static void WriteKazeNotify(double _time, KazeInfo _info)
        {
            writer.Reset();
            writer.Put(Message.KazeNotify);
            writer.Put(_time);
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

        public static void WriteDeathOrderOrReplication(double _time, DeathInfo _info)
        {
            writer.Reset();
            writer.Put(Message.DeathOrderOrReplication);
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

        public static void WriteGunShotReplication(double _time, GunShotInfo _info)
        {
            writer.Reset();
            writer.Put(Message.GunShotReplication);
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

        public static void WriteHitConfirmOrder(double _time)
        {
            writer.Reset();
            writer.Put(Message.HitConfirmOrder);
            writer.Put(_time);
        }
        #endregion

        #region Room messages
        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double)];

        public static void WriteTimeSync(double _time)
        {
            writer.Reset();
            writer.Put(Message.TimeSync);
            writer.Put(_time);
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            writer.Put(~bytes);
        }

        public static void WriteReady()
        {
            writer.Reset();
            writer.Put(Message.ReadyNotify);
        }

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

        public static void WriteQuitReplication(double _time, byte _id)
        {
            writer.Reset();
            writer.Put(Message.QuitReplication);
            writer.Put(_time);
            writer.Put(_id);
        }
        #endregion
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

        public DeathCause ReadDeathCauseType()
        {
            return ReadEnum<DeathCause>();
        }

        private KazeInfo ReadKazeInfo()
        {
            return new KazeInfo
            {
                position = ReadVector3()
            };
        }

        private DeathInfo ReadDeathInfo()
        {
            return new DeathInfo
            {
                deadId = ReadByte(),
                killerId = ReadByte(),
                cause = ReadDeathCauseType(),
                position = ReadVector3(),
                explosion = ReadBool()
            };
        }

        private SpawnInfo ReadSpawnInfo()
        {
            return new SpawnInfo
            {
                spawnPoint = ReadInt()
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
        
        private GunHitInfo ReadGunHitInfo()
        {
            return new GunHitInfo
            {
                id = ReadByte(),
                position = ReadVector3(),
                normal = ReadVector3()
            };
        }

        #endregion

        #region Movement messages

        public void ReadMovementNotify(out int _outStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _outSnapshot)
        {
            _outStep = ReadInt();
            _outSnapshot = ReadSnapshot();
            _outInputStepCount = ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = ReadInputStep();
            }
        }

        public void ReadSimulationCorrection(out int _outStep, out SimulationStepInfo _outSimulation)
        {
            _outStep = ReadInt();
            _outSimulation = ReadSimulationStepInfo();
        }

        public void ReadMovementReplication(out byte _id, out int _step, out Snapshot _snapshot)
        {
            _id = ReadByte();
            _step = ReadInt();
            _snapshot = ReadSnapshot();
        }

        public void ReadMovementAndInputReplication(out byte _outId, out int _outStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _snapshot)
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

        public void ReadSpawnOrder(out double _outTime, out byte _outSpawnPoint)
        {
            _outTime = ReadDouble();
            _outSpawnPoint = ReadByte();
        }

        public void ReadDeathOrder(out double _outTime)
        {
            _outTime = ReadDouble();
        }

        public void ReadSpawnReplication(out double _outTime, out byte _outId, out byte _outSpawnPoint)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnPoint = ReadByte();
            EnsureReadEnd();
        }

        public void ReadKazeNotify(out double _outTime, out KazeInfo _outKazeInfo)
        {
            _outTime = ReadDouble();
            _outKazeInfo = ReadKazeInfo();
        }

        public void ReadGunShotNotify(out double _outTime, out Vector3 _outPosition, out Sight _outSight, out bool _outIsRocket)
        {
            _outTime = ReadDouble();
            _outPosition = ReadVector3();
            _outSight= ReadSight();
            _outIsRocket = ReadBool();
        }

        public void ReadDeathOrderOrReplication(out double _outTime, out DeathInfo _outDeathInfo)
        {
            _outTime = ReadDouble();
            _outDeathInfo = ReadDeathInfo();
        }

        public void ReadSpawnOrderOrReplication(out double _outTime, out byte _outId, out SpawnInfo _outSpawnInfo)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
            _outSpawnInfo = ReadSpawnInfo();
        }

        public void ReadGunShotReplication(out double _outTime, out GunShotInfo _outGunShotInfo)
        {
            _outTime = ReadDouble();
            _outGunShotInfo = ReadGunShotInfo();
        }

        public void ReadGunHitOrderOrReplication(out double _outTime, out bool _outIsOrder, out GunHitInfo _outGunHitInfo)
        {
            _outTime = ReadDouble();
            _outIsOrder = ReadBool();
            _outGunHitInfo = ReadGunHitInfo();
        }

        public void ReadHitConfirmOrder(out double _outTime)
        {
            _outTime = ReadDouble();
        }
        #endregion

        #region Room messages

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double) + sizeof(ushort)];

        public void ReadTimeSync(out double _time)
        {
            _time = ReadDouble();
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            EnsureRead(bytes == ~ReadULong());
            EnsureReadEnd();
        }

        public void ReadPlayerSync(out double _outTime, out IEnumerable<PlayerRecapInfo> _outInfos)
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


        public void ReadPlayerWelcomeSync(out byte _outId)
        {
            _outId = ReadByte();
            // Checksum
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureRead(ReadByte() == _outId);
            EnsureRead(ReadByte() == 255 - _outId);
            EnsureReadEnd();
        }

        public void ReadPlayerIntroduction(out byte _outId, out PlayerInfo _outInfo)
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

        public void ReadQuitReplication(out double _outTime, out byte _outId)
        {
            _outTime = ReadDouble();
            _outId = ReadByte();
        }

        #endregion

    }

}
