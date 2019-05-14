using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;

namespace Wheeled.HUD
{
    internal sealed class EventBoardDispatcher : EventHistory<EventBoardDispatcher.IEvent>.ITarget
    {
        #region Public Interfaces

        public interface IEvent
        {
            #region Public Methods

            void Instantiate();

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Public Structs

        public struct JoinEvent : IEvent
        {
            #region Public Fields

            public IReadOnlyPlayer player;

            #endregion Public Fields

            #region Public Methods

            public void Instantiate()
            {
                IReadOnlyPlayer player = this.player;
                EventBoardBehaviour.AddMain(() =>
                {
                    return string.Format("{0} joined the game", player.GetColoredName());
                });
            }

            #endregion Public Methods
        }

        public struct KillEvent : IEvent
        {
            #region Public Fields

            public IReadOnlyPlayer killer;
            public IReadOnlyPlayer victim;
            public OffenseType offenseType;

            #endregion Public Fields

            #region Public Methods

            public void Instantiate()
            {
                IReadOnlyPlayer killer = this.killer;
                IReadOnlyPlayer victim = this.victim;
                string cause = null;
                switch (offenseType)
                {
                    case OffenseType.Rifle:
                    cause = "laser";
                    break;
                    case OffenseType.Rocket:
                    cause = "rocket";
                    break;
                    case OffenseType.Explosion:
                    cause = "explosion";
                    break;
                }
                string format;
                if (killer == victim)
                {
                    format = "{0} killed himself by {2}";
                }
                else
                {
                    format = "{0} killed {1} by {2}";
                }
                EventBoardBehaviour.AddMain(() =>
                {
                    return string.Format(format, killer.GetColoredName(), victim.GetColoredName(), cause);
                });
            }

            #endregion Public Methods
        }

        public struct QuitEvent : IEvent
        {
            #region Public Fields

            public IReadOnlyPlayer player;

            #endregion Public Fields

            #region Public Methods

            public void Instantiate()
            {
                IReadOnlyPlayer player = this.player;
                EventBoardBehaviour.AddMain(() =>
                {
                    return string.Format("{0} left the game", player.GetColoredName());
                });
            }

            #endregion Public Methods
        }

        #endregion Public Structs

        #region Private Fields

        private readonly EventHistory<IEvent> m_history;

        #endregion Private Fields

        #region Public Constructors

        public EventBoardDispatcher()
        {
            m_history = new EventHistory<IEvent>
            {
                Target = this
            };
        }

        #endregion Public Constructors

        #region Public Methods

        public void Put(double _time, IEvent _event)
        {
            m_history.Put(_time, _event);
        }

        public void UpdateUntil(double _time)
        {
            m_history.PerformUntil(_time);
        }

        void EventHistory<IEvent>.ITarget.Perform(double _time, IEvent _value)
        {
            _value.Instantiate();
        }

        #endregion Public Methods
    }
}