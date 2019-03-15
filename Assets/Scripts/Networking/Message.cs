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
        // TODO Optimization: Add messages for combined simulation and sight
        // Movement
        Simulation, SimulationCorrection, Sight, MovementReplication, SimulationAndSight,
        // Room
        RoomUpdate, Ready,
        // Actions
        SpawnOrder, DeathOrder, Shoot, ShootReplication, HitFeedback, HitNotify, Kaze
    }

    internal static class Serializer
    {

        #region NetDataWriter Put extension methods

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

        public static readonly NetDataWriter writer = new NetDataWriter(true, 128);

        public static void WriteSimulationMessage(int _firstStep, IReadOnlyList<InputStep> _steps, in SimulationStep _simulation)
        {
            writer.Reset();
            writer.Put(Message.Simulation);
            writer.Put(_firstStep);
            writer.Put(_simulation);
            writer.Put((byte) _steps.Count);
            foreach (InputStep inputStep in _steps)
            {
                writer.Put(inputStep);
            }
        }

        public static void WriteSightMessage(int _step, in Sight _sight)
        {
            writer.Reset();
            writer.Put(Message.Sight);
            writer.Put(_step);
            writer.Put(_sight);
        }

        public static void WriteSimulationCorrectionMessage(int _step, in SimulationStepInfo _simulationStepInfo)
        {
            writer.Reset();
            writer.Put(Message.SimulationCorrection);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        public static void WriteRoomUpdateMessage(in TimeStep _time /* Player stats and status */)
        {
            writer.Reset();
            writer.Put(Message.RoomUpdate);
            writer.Put(_time);
        }

        public static void WriteReadyMessage()
        {
            writer.Reset();
            writer.Put(Message.Ready);
        }

        public static void WriteMovementReplicationMessage(byte _id, int _step, in Sight _sight, in SimulationStep _simulation)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_sight);
            writer.Put(_simulation);
        }

        public static void WriteMovementReplicationMessage(byte _id, int _firstStep, in Sight _sight, IReadOnlyList<InputStep> _inputSteps, in SimulationStep _simulation)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_firstStep);
            writer.Put(_sight);
            writer.Put(_simulation);
            writer.Put((byte) _inputSteps.Count);
            foreach (InputStep inputStep in _inputSteps)
            {
                writer.Put(inputStep);
            }
        }

        public static void WriteSimulationAndSightMessage(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.SimulationAndSight);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put((byte) _inputSteps.Count);
            foreach (InputStep inputStep in _inputSteps)
            {
                writer.Put(inputStep);
            }
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

        private static SimulationStepInfo ReadSimulationStepInfo(this NetDataReader _netDataReader)
        {
            return new SimulationStepInfo
            {
                input = _netDataReader.ReadInputStep(),
                simulation = _netDataReader.ReadSimulationStep()
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

        public static void ReadSimulationMessage(this NetDataReader _netDataReader, out int _outFirstStep, InputStep[] _inputStepBuffer, out int _outInputStepCount, out SimulationStep _outSimulation)
        {
            _outFirstStep = _netDataReader.ReadInt();
            _outSimulation = _netDataReader.ReadSimulationStep();
            _outInputStepCount = _netDataReader.ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = _netDataReader.ReadInputStep();
            }
        }

        public static void ReadSightMessage(this NetDataReader _netDataReader, out int _outStep, out Sight _outSight)
        {
            _outStep = _netDataReader.ReadInt();
            _outSight = _netDataReader.ReadSight();
        }

        public static void ReadSimulationCorrectionMessage(this NetDataReader _netDataReader, out int _outStep, out SimulationStepInfo _outSimulation)
        {
            _outStep = _netDataReader.ReadInt();
            _outSimulation = _netDataReader.ReadSimulationStepInfo();
        }

        public static void ReadMovementReplicationMessage(this NetDataReader _netDataReader, out byte _id, out int _step, out Snapshot _snapshot)
        {
            _id = _netDataReader.ReadByte();
            _step = _netDataReader.ReadInt();
            _snapshot.sight = _netDataReader.ReadSight();
            _snapshot.simulation = _netDataReader.ReadSimulationStep();
        }

        public static void ReadMovementReplicationMessage(this NetDataReader _netDataReader, out byte _id, out int _firstStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _snapshot)
        {
            _id = _netDataReader.ReadByte();
            _firstStep = _netDataReader.ReadInt();
            _snapshot.sight = _netDataReader.ReadSight();
            _snapshot.simulation = _netDataReader.ReadSimulationStep();
            _outInputStepCount = _netDataReader.ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = _netDataReader.ReadInputStep();
            }
        }

        public static void ReadSimulationAndSightMessage(this NetDataReader _netDataReader, out int _firstStep, out int _outInputStepCount, InputStep[] _inputStepBuffer, out Snapshot _snapshot)
        {
            _firstStep = _netDataReader.ReadInt();
            _snapshot = _netDataReader.ReadSnapshot();
            _outInputStepCount = _netDataReader.ReadByte();
            for (int i = 0; i < _outInputStepCount && i < _inputStepBuffer.Length; i++)
            {
                _inputStepBuffer[i] = _netDataReader.ReadInputStep();
            }
        }

    }

}
