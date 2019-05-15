using System;
using UnityEngine.Events;

namespace Wheeled.Menu
{
    public sealed class PortValidatorBehaviour : ValidatorBehaviour
    {
        [Serializable]
        public sealed class ChangedEvent : UnityEvent<int?> { }

        public ChangedEvent changed;

        public static int? ParsePort(string _string)
        {
            if (int.TryParse(_string, out int port))
            {
                if (port > 1023 && port < 65536)
                {
                    return port;
                }
            }
            return null;
        }

        public static bool IsValidPort(string _string)
        {
            return ParsePort(_string) != null;
        }

        public void Validate(string _string)
        {
            int? port = ParsePort(_string);
            validated.Invoke(port != null);
            changed.Invoke(port);
        }
    }
}