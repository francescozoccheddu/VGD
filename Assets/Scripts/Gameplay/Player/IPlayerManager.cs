using Wheeled.Gameplay.Stage;
using Wheeled.HUD;

namespace Wheeled.Gameplay.Player
{
    internal interface IPlayerManager
    {
        #region Public Properties

        OffenseBackstage OffenseBackstage { get; }
        MatchBoard MatchBoard { get; }
        double Time { get; }

        #endregion Public Properties
    }
}