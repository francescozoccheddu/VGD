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
        Movement, MovementReplication, MovementCorrection, RoomUpdate
    }

    internal static class Serializer
    {

        #region NetDataWriter Put extension methods

        private static void Put(this NetDataWriter _netDataWriter, Enum _value)
        {
            _netDataWriter.Put(Convert.ToByte(_value));
        }

        private static void Put(this NetDataWriter _netDataWriter, Vector3 _value)
        {
            _netDataWriter.Put(_value.x);
            _netDataWriter.Put(_value.y);
            _netDataWriter.Put(_value.z);
        }

        private static void Put(this NetDataWriter _netDataWriter, InputStep _value)
        {
            _netDataWriter.Put(_value.dash);
            _netDataWriter.Put(_value.jump);
            _netDataWriter.Put(_value.movementX);
            _netDataWriter.Put(_value.movementZ);
        }

        private static void Put(this NetDataWriter _netDataWriter, SimulationStep _value)
        {
            _netDataWriter.Put(_value.velocity);
            _netDataWriter.Put(_value.position);
        }

        private static void Put(this NetDataWriter _netDataWriter, Sight _value)
        {
            _netDataWriter.Put(_value.Turn);
            _netDataWriter.Put(_value.LookUp);
        }

        private static void Put(this NetDataWriter _netDataWriter, Snapshot _value)
        {
            _netDataWriter.Put(_value.simulation);
            _netDataWriter.Put(_value.sight);
        }

        private static void Put(this NetDataWriter _netDataWriter, TimeStep _value)
        {
            _netDataWriter.Put(_value.Step);
            _netDataWriter.Put(_value.Remainder);
        }

        #endregion

        public static readonly NetDataWriter writer = new NetDataWriter(true, 128);

        public static void WriteMovementMessage(int _firstStep, IReadOnlyList<InputStep> _steps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.Movement);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put((byte) _steps.Count);
            foreach (InputStep inputStep in _steps)
            {
                writer.Put(inputStep);
            }
        }

        public static void WriteRoomUpdateMessage(TimeStep _time /* Player stats and status */)
        {
            writer.Reset();
            writer.Put(Message.RoomUpdate);
            writer.Put(_time);
        }

    }

    internal static class Deserializer
    {

        public sealed class DeserializationException : Exception { }

        private static void EnsureRead(bool _read)
        {
            if (!_read)
            {
                throw new DeserializationException();
            }
        }

        private static bool ReadBool(this NetDataReader _netDataReader)
        {
            EnsureRead(_netDataReader.TryGetBool(out bool value));
            return value;
        }

        private static float ReadFloat(this NetDataReader _netDataReader)
        {
            EnsureRead(_netDataReader.TryGetFloat(out float value));
            return value;
        }

        private static uint ReadUint(this NetDataReader _netDataReader)
        {
            EnsureRead(_netDataReader.TryGetUInt(out uint value));
            return value;
        }

        private static int ReadInt(this NetDataReader _netDataReader)
        {
            EnsureRead(_netDataReader.TryGetInt(out int value));
            return value;
        }

        private static byte ReadByte(this NetDataReader _netDataReader)
        {
            EnsureRead(_netDataReader.TryGetByte(out byte value));
            return value;
        }

        private static string ReadString(this NetDataReader _netDataReader)
        {
            EnsureRead(_netDataReader.TryGetString(out string value));
            return value;
        }

        private static T ReadEnum<T>(this NetDataReader _netDataReader) where T : Enum
        {
            byte b = _netDataReader.ReadByte();
            object value = Enum.ToObject(typeof(T), b);
            EnsureRead(Enum.IsDefined(typeof(T), value));
            return (T) value;
        }

        private static Vector3 ReadVector3(this NetDataReader _netDataReader)
        {
            return new Vector3
            {
                x = _netDataReader.ReadFloat(),
                y = _netDataReader.ReadFloat(),
                z = _netDataReader.ReadFloat()
            };
        }

        private static InputStep ReadInputStep(this NetDataReader _netDataReader)
        {
            return new InputStep
            {
                dash = _netDataReader.ReadBool(),
                jump = _netDataReader.ReadBool(),
                movementX = _netDataReader.ReadFloat(),
                movementZ = _netDataReader.ReadFloat()
            };
        }

        private static SimulationStep ReadSimulationStep(this NetDataReader _netDataReader)
        {
            return new SimulationStep
            {
                velocity = _netDataReader.ReadVector3(),
                position = _netDataReader.ReadVector3()
            };
        }

        private static Sight ReadSight(this NetDataReader _netDataReader)
        {
            return new Sight
            {
                Turn = _netDataReader.ReadFloat(),
                LookUp = _netDataReader.ReadFloat(),
            };
        }

        private static Snapshot ReadSnapshot(this NetDataReader _netDataReader)
        {
            return new Snapshot
            {
                simulation = _netDataReader.ReadSimulationStep(),
                sight = _netDataReader.ReadSight()
            };
        }

        public static Message ReadMessageType(this NetDataReader _netDataReader)
        {
            return _netDataReader.ReadEnum<Message>();
        }

        public static TimeStep ReadTime(this NetDataReader _netDataReader)
        {
            return new TimeStep
            {
                Step = _netDataReader.ReadInt(),
                Remainder = _netDataReader.ReadFloat()
            };
        }

        public static void ReadRoomUpdateMessage(this NetDataReader _netDataReader, out TimeStep _time)
        {
            _time = _netDataReader.ReadTime();
        }

        public static void ReadMovementMessage(this NetDataReader _netDataReader, out int _outFirstStep, InputStep[] _inputStepBuffer, out int _outInputStepCount, out Snapshot _outSnapshot)
        {
            _outFirstStep = _netDataReader.ReadInt();
            _outSnapshot = _netDataReader.ReadSnapshot();
            _outInputStepCount = _netDataReader.ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = _netDataReader.ReadInputStep();
            }
        }

    }

}
