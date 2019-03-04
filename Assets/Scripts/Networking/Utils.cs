using LiteNetLib.Utils;
using System;
using UnityEngine;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    public class NetworkException : Exception { }

    public static class Utils
    {

        public static void Put(this NetDataWriter netDataWriter, Enum _value)
        {
            netDataWriter.Put(Convert.ToByte(_value));
        }

        public static T GetEnum<T>(this NetDataReader netDataReader) where T : Enum
        {
            byte b = netDataReader.GetByte();
            return (T) Enum.ToObject(typeof(T), b);
        }

        public static void Put(this NetDataWriter netDataWriter, Vector3 _value)
        {
            netDataWriter.Put(_value.x);
            netDataWriter.Put(_value.y);
            netDataWriter.Put(_value.z);
        }

        public static Vector3 GetVector3(this NetDataReader netDataReader)
        {
            return new Vector3
            {
                x = netDataReader.GetFloat(),
                y = netDataReader.GetFloat(),
                z = netDataReader.GetFloat()
            };
        }

        public static void Put(this NetDataWriter netDataWriter, PlayerBehaviour.InputState _value)
        {
            netDataWriter.Put(_value.dash);
            netDataWriter.Put(_value.jump);
            netDataWriter.Put(_value.movementX);
            netDataWriter.Put(_value.movementZ);
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

        public static void Put(this NetDataWriter netDataWriter, PlayerBehaviour.SimulationState _value)
        {
            netDataWriter.Put(_value.dashStamina);
            netDataWriter.Put(_value.lookUp);
            netDataWriter.Put(_value.turn);
            netDataWriter.Put(_value.velocity);
            netDataWriter.Put(_value.position);
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

    }

}
