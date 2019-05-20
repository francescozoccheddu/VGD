using System;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine.Events;

namespace Wheeled.UI.Menu
{
    public sealed class PortValidatorBehaviour : ValidatorBehaviour
    {
        [Serializable]
        public sealed class ChangedEvent : UnityEvent<int?> { }

        public bool ensureNotInUse;

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

        public static bool IsInUse(int _port) => IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(_p => _p.Port == _port);

        public static bool IsValidPort(string _string) => ParsePort(_string) != null;

        public void Validate(string _string)
        {
            int? port = ParsePort(_string);
            if (port != null && ensureNotInUse)
            {
                validated.Invoke(!IsInUse(port.Value));
            }
            else
            {
                validated.Invoke(port != null);
            }
            changed.Invoke(port);
        }
    }
}