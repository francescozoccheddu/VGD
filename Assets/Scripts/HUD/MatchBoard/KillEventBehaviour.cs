using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;

namespace Wheeled.HUD
{
    public sealed class KillEventBehaviour : MatchBoardEventBehaviour
    {
        #region Internal Properties

        internal IReadOnlyPlayer Killer { get; set; }
        internal IReadOnlyPlayer Victim { get; set; }
        internal OffenseType OffenseType { get; set; }

        #endregion Internal Properties

        #region Protected Methods

        protected override string GetText()
        {
            string offense = null;
            switch (OffenseType)
            {
                case OffenseType.Rifle:
                offense = "rifle";
                break;

                case OffenseType.Rocket:
                offense = "rocket";
                break;

                case OffenseType.Explosion:
                offense = "explosion";
                break;
            }
            return string.Format("{0} killed {1} by {2}", GetName(Killer?.Info?.name), GetName(Victim?.Info?.name), offense);
        }

        #endregion Protected Methods
    }
}