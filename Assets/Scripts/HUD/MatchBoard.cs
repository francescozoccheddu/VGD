using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;

namespace Wheeled.HUD
{
    internal sealed class MatchBoard : EventHistory<MatchBoard.IEvent>.ITarget
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
                JoinEventBehaviour behaviour = Object.Instantiate(ScriptManager.Actors.joinEvent).GetComponent<JoinEventBehaviour>();
                behaviour.Player = player;
                MatchBoardBehaviour.Add(behaviour);
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
                KillEventBehaviour behaviour = Object.Instantiate(ScriptManager.Actors.killEvent).GetComponent<KillEventBehaviour>();
                behaviour.Killer = killer;
                behaviour.Victim = victim;
                behaviour.OffenseType = offenseType;
                MatchBoardBehaviour.Add(behaviour);
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
                QuitEventBehaviour behaviour = Object.Instantiate(ScriptManager.Actors.quitEvent).GetComponent<QuitEventBehaviour>();
                behaviour.Player = player;
                MatchBoardBehaviour.Add(behaviour);
            }

            #endregion Public Methods
        }

        #endregion Public Structs

        #region Private Fields

        private readonly EventHistory<IEvent> m_history;

        #endregion Private Fields

        #region Public Constructors

        public MatchBoard()
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