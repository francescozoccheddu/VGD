using LiteNetLib.Utils;
using System;
using UnityEngine;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    public class NetworkException : Exception { }

    public static class Utils
    {

        public static void Put(this NetDataWriter _netDataWriter, Enum _value)
        {
            _netDataWriter.Put(Convert.ToByte(_value));
        }

        public static T GetEnum<T>(this NetDataReader _netDataReader) where T : Enum
        {
            byte b = _netDataReader.GetByte();
            return (T) Enum.ToObject(typeof(T), b);
        }

        public static void Put(this NetDataWriter _netDataWriter, Vector3 _value)
        {
            _netDataWriter.Put(_value.x);
            _netDataWriter.Put(_value.y);
            _netDataWriter.Put(_value.z);
        }

        public static Vector3 GetVector3(this NetDataReader _netDataReader)
        {
            return new Vector3
            {
                x = _netDataReader.GetFloat(),
                y = _netDataReader.GetFloat(),
                z = _netDataReader.GetFloat()
            };
        }

        public static void Put(this NetDataWriter _netDataWriter, PlayerBehaviour.InputState _value)
        {
            _netDataWriter.Put(_value.dash);
            _netDataWriter.Put(_value.jump);
            _netDataWriter.Put(_value.movementX);
            _netDataWriter.Put(_value.movementZ);
        }

        public static PlayerBehaviour.InputState GetInputState(this NetDataReader _netDataReader)
        {
            return new PlayerBehaviour.InputState
            {
                dash = _netDataReader.GetBool(),
                jump = _netDataReader.GetBool(),
                movementX = _netDataReader.GetFloat(),
                movementZ = _netDataReader.GetFloat()
            };
        }

        public static void Put(this NetDataWriter _netDataWriter, PlayerBehaviour.SimulationState _value)
        {
            _netDataWriter.Put(_value.dashStamina);
            _netDataWriter.Put(_value.lookUp);
            _netDataWriter.Put(_value.turn);
            _netDataWriter.Put(_value.velocity);
            _netDataWriter.Put(_value.position);
        }

        public static PlayerBehaviour.SimulationState GetSimulationState(this NetDataReader _netDataReader)
        {
            return new PlayerBehaviour.SimulationState
            {
                dashStamina = _netDataReader.GetFloat(),
                lookUp = _netDataReader.GetFloat(),
                turn = _netDataReader.GetFloat(),
                velocity = _netDataReader.GetVector3(),
                position = _netDataReader.GetVector3()
            };
        }

        public static void Put(this NetDataWriter _netDataWriter, PlayerBehaviour.Time _value)
        {
            _netDataWriter.Put(_value.Node);
            _netDataWriter.Put(_value.TimeSinceNode);
        }

        public static PlayerBehaviour.Time GetTime(this NetDataReader _netDataReader)
        {
            return new PlayerBehaviour.Time(_netDataReader.GetInt(), _netDataReader.GetFloat());
        }

    }

}
