using Wheeled.Core.Data;

namespace Wheeled.UI.Menu
{
    public sealed class NameValidatorBehaviour : ValidatorBehaviour
    {
        public void Validate(string _string)
        {
            validated.Invoke(PlayerPreferences.IsValidName(_string));
        }
    }
}