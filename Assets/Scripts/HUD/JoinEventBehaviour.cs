﻿using Wheeled.Gameplay.Player;

namespace Wheeled.HUD
{
    public sealed class JoinEventBehaviour : EventBehaviour
    {
        #region Internal Properties

        internal IReadOnlyPlayer Player { get; set; }

        #endregion Internal Properties

        #region Protected Methods

        protected override string GetText()
        {
            return "";
        }

        #endregion Protected Methods
    }
}