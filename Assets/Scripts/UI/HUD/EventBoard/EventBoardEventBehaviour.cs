using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public sealed class EventBoardEventBehaviour : MonoBehaviour
    {

        public Text text;

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public delegate string TextProvider();

        public TextProvider MessageProvider { get; set; }

        private void Update()
        {
            text.text= MessageProvider();
        }

    }
}