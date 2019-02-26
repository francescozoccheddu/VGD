using LiteNetLib.Utils;
using System;
using UnityEngine;

namespace Wheeled.Networking
{

    public class NetworkException : Exception { }

    public static class Utils
    {

        public static void Put(this NetDataWriter netDataWriter, Enum @enum)
        {
            netDataWriter.Put(Convert.ToByte(@enum));
        }

        public static T GetEnum<T>(this NetDataReader netDataReader) where T : Enum
        {
            byte b = netDataReader.GetByte();
            return (T) Enum.ToObject(typeof(T), b);
        }

        private static void Put(this NetDataWriter netDataWriter, Vector3 vector)
        {
            netDataWriter.Put(vector.x);
            netDataWriter.Put(vector.y);
            netDataWriter.Put(vector.z);
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

    }

}
