using System.Net;
using System.Net.Sockets;

namespace Wheeled.Menu
{
    public sealed class IPValidatorBehaviour : ValidatorBehaviour
    {
        #region Public Methods

        public static bool IsValidIP(string _string)
        {
            if (IPAddress.TryParse(_string, out IPAddress address))
            {
                return address.AddressFamily == AddressFamily.InterNetwork;
            }
            return false;
        }

        public void Validate(string _string)
        {
            validated.Invoke(IsValidIP(_string));
        }

        #endregion Public Methods
    }
}