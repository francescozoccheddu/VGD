using UnityEngine;
using Wheeled.Core;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;

namespace Wheeled.HUD
{
    public sealed class EventBoardBehaviour : MonoBehaviour, EventHistory<EventBoardBehaviour.IEvent>.ITarget
    {

        public interface IEvent
        {
            void Instantiate(EventBoardBehaviour _eventBoard);
        }

        public struct JoinEvent : IEvent
        {
            public IReadOnlyPlayer player;

            public void Instantiate(EventBoardBehaviour _eventBoard)
            {
                IReadOnlyPlayer player = this.player;
                _eventBoard.Add(() =>
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

            public void Instantiate(EventBoardBehaviour _eventBoard)
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
                _eventBoard.Add(() =>
                {
                    return string.Format(format, killer.GetColoredName(), victim.GetColoredName(), cause);
                });
            }
        }

        public struct QuitEvent : IEvent
        {
            public IReadOnlyPlayer player;

            public void Instantiate(EventBoardBehaviour _eventBoard)
            {
                IReadOnlyPlayer player = this.player;
                _eventBoard.Add(() =>
                {
                    return string.Format("{0} left the game", player.GetColoredName());
                });
            }
        }

        private readonly EventHistory<IEvent> m_history;

        public EventBoardBehaviour()
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

        void EventHistory<IEvent>.ITarget.Perform(double _time, IEvent _value)
        {
            _value.Instantiate(this);
        }

        public static EventBoardBehaviour Instance { get; private set; }

        private void Add(EventBoardEventBehaviour.TextProvider _messageProvider)
        {
            EventBoardEventBehaviour gameObject = Instantiate(eventPrefab, transform).GetComponent<EventBoardEventBehaviour>();
            gameObject.MessageProvider = _messageProvider;
        }

        public GameObject eventPrefab;

        private void Start()
        {
            Instance = this;
        }

        private void Update()
        {
            m_history.PerformUntil(GameManager.Current.Time);
        }

    }
}