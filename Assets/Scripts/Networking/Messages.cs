using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking
{

    internal enum Message
    {
        // Movement
        MovementNotify, SimulationCorrection, MovementReplication, MovementAndInputReplication,
        // Room
        RoomUpdate, Ready,
        // Actions
        SpawnOrder, DeathOrder, ShootNotify, ShootReplication, HitFeedback, HitNotify, Kaze
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

        private static void Put(this NetDataWriter _netDataWriter, in TimeStep _value)
        {
            _netDataWriter.Put(_value.Step);
            _netDataWriter.Put(_value.Remainder);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SimulationStepInfo _value)
        {
            _netDataWriter.Put(_value.input);
            _netDataWriter.Put(_value.simulation);
        }

        #endregion

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
            writer.Put(Message.SimulationCorrection);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(float) + sizeof(int)];

        public static void WriteRoomUpdateMessage(in TimeStep _time /* Player stats and status */)
        {
            writer.Reset();
            writer.Put(Message.RoomUpdate);
            writer.Put(_time);
            {
                FastBitConverter.GetBytes(s_timeChecksumBuffer, 0, _time.Step);
                FastBitConverter.GetBytes(s_timeChecksumBuffer, sizeof(int), _time.Remainder);
                ushort crc = CRC16.Compute(s_timeChecksumBuffer);
                writer.Put(crc);
            }
        }

        public static void WriteReadyMessage()
        {
            writer.Reset();
            writer.Put(Message.Ready);
        }

        public static void WriteMovementReplicationMessage(byte _id, int _step, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
        }

        public static void WriteMovementAndInputReplicationMessage(byte _id, int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementAndInputReplication);
            writer.Put(_id);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put((byte) _inputSteps.Count);
            foreach (InputStep inputStep in _inputSteps)
            {
                writer.Put(inputStep);
            }
        }

    }

    internal sealed class Deserializer
    {

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

        private ushort ReadUshort()
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

        public Message ReadMessageType()
        {
            return ReadEnum<Message>();
        }

        public TimeStep ReadTime()
        {
            return new TimeStep
            {
                Step = ReadInt(),
                Remainder = ReadFloat()
            };
        }

        #endregion

        #region Message read methods

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(float) + sizeof(int) + sizeof(ushort)];

        public void ReadRoomUpdateMessage(out TimeStep _time)
        {
            _time = ReadTime();
            {
                ushort crc = ReadUshort();
                FastBitConverter.GetBytes(s_timeChecksumBuffer, 0, _time.Step);
                FastBitConverter.GetBytes(s_timeChecksumBuffer, sizeof(int), _time.Remainder);
                FastBitConverter.GetBytes(s_timeChecksumBuffer, sizeof(int) + sizeof(float), crc);
                EnsureRead(CRC16.Compute(s_timeChecksumBuffer) == 0);
            }
        }

        public void ReadMovementNotifyMessage(out int _outFirstStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _outSnapshot)
        {
            _outFirstStep = ReadInt();
            _outSnapshot = ReadSnapshot();
            _outInputStepCount = ReadByte();
            int j = 0;
            for (int i = 0; i < _outInputStepCount; i++)
            {
                InputStep inputStep = ReadInputStep();
                if (i >= _outInputStepCount - _inputStepBuffer.Length)
                {
                    _inputStepBuffer[j++] = inputStep;
                }
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
            _snapshot.sight = ReadSight();
            _snapshot.simulation = ReadSimulationStep();
        }

        public void ReadMovementAndInputReplicationMessage(out byte _id, out int _firstStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _snapshot)
        {
            _id = ReadByte();
            _firstStep = ReadInt();
            _snapshot.sight = ReadSight();
            _snapshot.simulation = ReadSimulationStep();
            _outInputStepCount = ReadByte();
            int j = 0;
            for (int i = 0; i < _outInputStepCount; i++)
            {
                InputStep inputStep = ReadInputStep();
                if (i >= _outInputStepCount - _inputStepBuffer.Length)
                {
                    _inputStepBuffer[j++] = inputStep;
                }
            }
        }

        #endregion

    }

}
