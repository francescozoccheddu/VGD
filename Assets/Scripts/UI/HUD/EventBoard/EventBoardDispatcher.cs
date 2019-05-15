using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;

namespace Wheeled.HUD
{
    public sealed class EventBoardDispatcher : EventHistory<EventBoardDispatcher.IEvent>.ITarget
    {
        public interface IEvent
        {
            void Instantiate();
        }

        public struct JoinEvent : IEvent
        {
            public IReadOnlyPlayer player;

            public void Instantiate()
            {
                IReadOnlyPlayer player = this.player;
                EventBoardBehaviour.AddMain(() =>
                {
                    return string.Format("{0} joined the game", player.GetColoredName());
                });
            }
        }

        public struct KillEvent : IEvent
        {
            public IReadOnlyPlayer killer;
            public IReadOnlyPlayer victim;
            public EOffenseType offenseType;

            public void Instantiate()
            {
                IReadOnlyPlayer killer = this.killer;
                IReadOnlyPlayer victim = this.victim;
                string cause = null;
                switch (offenseType)
                {
                    case EOffenseType.Rifle:
                    cause = "laser";
                    break;
                    case EOffenseType.Rocket:
                    cause = "rocket";
                    break;
                    case EOffenseType.Explosion:
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
        }

        public struct QuitEvent : IEvent
        {
            public IReadOnlyPlayer player;

            public void Instantiate()
            {
                IReadOnlyPlayer player = this.player;
                EventBoardBehaviour.AddMain(() =>
                {
                    return string.Format("{0} left the game", player.GetColoredName());
                });
            }
        }

        private readonly EventHistory<IEvent> m_history;

        public EventBoardDispatcher()
        {
            m_history = new EventHistory<IEvent>
            {
                Target = this
            };
        }

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
    }
}