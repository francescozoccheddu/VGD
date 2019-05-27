using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.HUD
{
    public sealed class EventBoardEventBehaviour : MonoBehaviour
    {

        public Text text;

        public AudioClip killSound;
        public AudioClip joinSound;
        public AudioClip quitSound;

        public void Destroy() => Destroy(gameObject);

        public enum EEventType
        {
            Kill, Join, Quit
        }

        public void PlaySound(EEventType _type)
        {
            AudioClip clip;
            switch (_type)
            {
                case EEventType.Kill:
                clip = killSound;
                break;
                case EEventType.Join:
                clip = joinSound;
                break;
                case EEventType.Quit:
                clip = quitSound;
                break;
                default:
                return;
            }
            GetComponentInParent<UIAudioSourceBehaviour>().Play(clip);
        }

        public delegate string TextProvider();

        public TextProvider MessageProvider { get; set; }

        private void Update() => text.text = MessageProvider();

    }
}