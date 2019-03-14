using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Debugging
{
    public static class Printer
    {

        private static Helper s_printer;

        private sealed class Helper : MonoBehaviour
        {
            private const float c_positionX = 5;
            private const float c_positionY = 5;
            private const float c_height = 22;
            private const float c_width = 200;

            public readonly Dictionary<string, object> values = new Dictionary<string, object>();

            private void OnGUI()
            {
                int i = 0;
                foreach (KeyValuePair<string, object> entry in values)
                {
                    float y = c_positionY + c_height * i++;
                    string text = string.Format("{0} = {1}", entry.Key, entry.Value);
                    GUI.Label(new Rect(c_positionX, y, c_width, c_height), text);
                }
            }

        }

        private static void EnsureCreated()
        {
            if (s_printer == null)
            {
                GameObject gameObject = new GameObject
                {
                    name = "Debug printer"
                };
                Object.DontDestroyOnLoad(gameObject);
                s_printer = gameObject.AddComponent<Helper>();
            }
        }

        public static void Debug(string _name, object _value)
        {
            EnsureCreated();
            s_printer.values[_name] = _value;
        }

        public static void DebugIncrement(string _name)
        {
            EnsureCreated();
            if (s_printer.values.TryGetValue(_name, out object value) && value is int intval)
            {
                s_printer.values[_name] = intval + 1;
            }
            else
            {
                s_printer.values[_name] = 0;
            }
        }

        public static object DebugGet(string _name)
        {
            EnsureCreated();
            if (s_printer.values.TryGetValue(_name, out object value))
            {
                return value;
            }
            return null;
        }

        public static void Clear()
        {
            EnsureCreated();
            s_printer.values.Clear();
        }

    }
}
