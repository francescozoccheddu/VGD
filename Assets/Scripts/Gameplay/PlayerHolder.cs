using UnityEngine;
using Wheeled.Networking;

namespace Wheeled.Gameplay
{

    public sealed class PlayerHolder : MonoBehaviour
    {

        public static void Spawn()
        {
            new GameObject("PlayerHolder").AddComponent<PlayerHolder>();
        }

        private readonly InteractivePlayer m_interactive;
        private readonly PlayerView m_view;

        public PlayerHolder()
        {
            m_interactive = new InteractivePlayer();
            m_view = new PlayerView();
        }

        private void Start()
        {
            m_interactive.StartAt(RoomTime.Time, TimeStep.zero, false);
        }

        private void Update()
        {
            m_interactive.Update();
            m_view.Move(m_interactive.ViewSnapshot);
        }

    }

}
