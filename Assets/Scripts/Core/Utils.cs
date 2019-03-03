using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Core
{
    internal static class Utils
    {

        public static IEnumerable<T> SingletonEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static void Put(this NetDataWriter _writer, Vector3 _value)
        {
            _writer.Put(_value.x);
            _writer.Put(_value.y);
            _writer.Put(_value.z);
        }

        public static Vector3 GetVector3(this NetDataReader _reader)
        {
            Vector3 value;
            value.x = _reader.GetFloat();
            value.y = _reader.GetFloat();
            value.z = _reader.GetFloat();
            return value;
        }

    }

    internal class InnerClass<T>
    {

        protected readonly T m_parent;

        protected InnerClass(T _parent)
        {
            m_parent = _parent;
        }

    }

}
