using LiteNetLib.Utils;
using System;
using UnityEngine;

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

    }

}
