namespace Wheeled.Menu
{
    public sealed class PortValidatorBehaviour : ValidatorBehaviour
    {
        #region Public Methods

        public static bool IsValidPort(string _string)
        {
            if (int.TryParse(_string, out int port))
            {
                return port > 1023 && port < 65536;
            }
            return false;
        }

        public void Validate(string _string)
        {
            validated.Invoke(IsValidPort(_string));
        }

        #endregion Public Methods
    }
}