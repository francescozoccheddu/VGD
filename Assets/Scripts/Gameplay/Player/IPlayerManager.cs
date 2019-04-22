using Wheeled.HUD;

namespace Wheeled.Gameplay.Player
{
    internal interface IPlayerManager
    {
        #region Public Properties

        MatchBoard MatchBoard { get; }
        double Time { get; }

        #endregion Public Properties
    }
}