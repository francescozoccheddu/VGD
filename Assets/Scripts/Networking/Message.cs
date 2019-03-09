using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    internal enum Message
    {
        Moved, Welcome, Corrected, Spawned, Died
    }

    internal static class Serializer
    {

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

        private static void Put(this NetDataWriter _netDataWriter, Snapshot _value)
        {
            _netDataWriter.Put(_value.simulation);
            _netDataWriter.Put(_value.Turn);
            _netDataWriter.Put(_value.LookUp);
        }

        public static readonly NetDataWriter writer = new NetDataWriter(true, 128);

        public static void WriteInteractivePlayerData(int _firstStep, IReadOnlyList<InputStep> _steps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put((byte) _steps.Count);
            foreach (InputStep inputStep in _steps)
            {
                writer.Put(inputStep);
            }
        }

    }

    internal static class Deserializer
    {

        // TODO Implement GetBool, GetInt, GetByte, GetFloat so that if deserialization fails, DeserializationException is thrown

        public sealed class DeserializationException : Exception { }

        // TODO If deserialization fails throw DeserializationException

        private static T GetEnum<T>(this NetDataReader _netDataReader) where T : Enum
        {
            byte b = _netDataReader.GetByte();
            return (T) Enum.ToObject(typeof(T), b);
        }

        private static Vector3 GetVector3(this NetDataReader _netDataReader)
        {
            return new Vector3
            {
                x = _netDataReader.GetFloat(),
                y = _netDataReader.GetFloat(),
                z = _netDataReader.GetFloat()
            };
        }

        private static InputStep GetInputStep(this NetDataReader _netDataReader)
        {
            return new InputStep
            {
                dash = _netDataReader.GetBool(),
                jump = _netDataReader.GetBool(),
                movementX = _netDataReader.GetFloat(),
                movementZ = _netDataReader.GetFloat()
            };
        }

        private static SimulationStep GetSimulationStep(this NetDataReader _netDataReader)
        {
            return new SimulationStep
            {
                velocity = _netDataReader.GetVector3(),
                position = _netDataReader.GetVector3()
            };
        }

        private static Snapshot GetSnapshot(this NetDataReader _netDataReader)
        {
            return new Snapshot
            {
                simulation = _netDataReader.GetSimulationStep(),
                Turn = _netDataReader.GetFloat(),
                LookUp = _netDataReader.GetFloat(),
            };
        }

        public static void ReadInteractivePlayerData(this NetDataReader _netDataReader, out int _outFirstStep, InputStep[] _inputStepBuffer, out Snapshot _outSnapshot)
        {
            _outFirstStep = _netDataReader.GetInt();
            _outSnapshot = _netDataReader.GetSnapshot();
            int inputStepCount = _netDataReader.GetByte();
            if (inputStepCount > _inputStepBuffer.Length)
            {
                throw new DeserializationException();
            }
            for (int i = 0; i < inputStepCount; i++)
            {
                _inputStepBuffer[i] = _netDataReader.GetInputStep();
            }
        }

    }

}
