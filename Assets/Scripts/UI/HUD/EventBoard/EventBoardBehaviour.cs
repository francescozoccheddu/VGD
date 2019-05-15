using UnityEngine;

namespace Wheeled.HUD
{
    public sealed class EventBoardBehaviour : MonoBehaviour
    {

        private static EventBoardBehaviour s_instance;
        public static void AddMain(EventBoardEventBehaviour.TextProvider _messageProvider)
        {
            s_instance.Add(_messageProvider);
        }

        public void Add(EventBoardEventBehaviour.TextProvider _messageProvider)
        {
            EventBoardEventBehaviour gameObject = Instantiate(eventPrefab, transform).GetComponent<EventBoardEventBehaviour>();
            gameObject.MessageProvider = _messageProvider;
        }
        public GameObject eventPrefab;

        private void Start()
        {
            s_instance = this;
        }

    }
}