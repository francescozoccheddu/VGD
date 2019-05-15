using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class NameValidatorBehaviour : ValidatorBehaviour
    {
        public void Validate(string _string)
        {
            validated.Invoke(PlayerPreferences.IsValidName(_string));
        }
    }
}