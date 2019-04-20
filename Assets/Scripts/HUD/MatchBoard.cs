using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;

namespace Wheeled.Assets.Scripts.HUD
{
    internal sealed class MatchBoard
    {
        #region Public Interfaces

        public interface IEvent
        {
        }

        #endregion Public Interfaces

        #region Public Structs

        public struct JoinEvent : IEvent
        {
            #region Public Fields

            public IReadOnlyPlayer player;

            #endregion Public Fields
        }

        public struct KillEvent : IEvent
        {
            #region Public Fields

            public IReadOnlyPlayer killer;
            public IReadOnlyPlayer victim;
            public OffenseType offenseType;

            #endregion Public Fields
        }

        public struct QuitEvent : IEvent
        {
            #region Public Fields

            public IReadOnlyPlayer player;

            #endregion Public Fields
        }

        #endregion Public Structs

        #region Private Fields

        private readonly LinkedListHistory<double, IEvent> m_history;

        #endregion Private Fields

        #region Public Constructors

        public MatchBoard()
        {
            m_history = new LinkedListHistory<double, IEvent>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Put(double _time, IEvent _event)
        {
            m_history.Add(_time, _event);
        }

        #endregion Public Methods
    }
}