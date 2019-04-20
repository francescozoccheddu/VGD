using Wheeled.Gameplay.Player;

namespace Wheeled.HUD
{
    public sealed class JoinEventBehaviour : MatchBoardEventBehaviour
    {
        #region Internal Properties

        internal IReadOnlyPlayer Player { get; set; }

        #endregion Internal Properties

        #region Protected Methods

        protected override string GetText()
        {
            return string.Format("<b>{0}</b> joined the game", Player?.Info?.name);
        }

        #endregion Protected Methods
    }
}