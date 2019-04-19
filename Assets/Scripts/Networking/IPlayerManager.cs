using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking
{
    internal interface IPlayerManager
    {
        #region Public Properties

        OffenseBackstage OffenseBackstage { get; }
        double Time { get; }

        #endregion Public Properties
    }
}