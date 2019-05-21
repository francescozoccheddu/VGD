using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Events;

namespace Wheeled.UI.Menu
{

    public sealed class IPValidatorBehaviour : ValidatorBehaviour
    {
        [Serializable]
        public sealed class ChangedEvent : UnityEvent<IPAddress> { }

        public ChangedEvent changed;

        public static bool IsValidIP(string _string)
        {
            return ParseIP(_string) != null;
        }
        public static IPAddress ParseIP(string _string)
        {
            if (IPAddress.TryParse(_string, out IPAddress address))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address;
                }
            }
            return null;
        }

        public void Validate(string _string)
        {
            var ip = ParseIP(_string);
            validated.Invoke(ip != null);
            changed.Invoke(ip);
        }
    }
}