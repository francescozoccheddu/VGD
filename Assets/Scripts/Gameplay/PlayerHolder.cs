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
            if (Input.GetKeyDown(KeyCode.T))
            {
                m_interactive.Teleport(new Snapshot(), false);
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                m_interactive.StartAt(RoomTime.Time, TimeStep.zero, false);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                m_interactive.Pause(true);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                m_interactive.StartAt(RoomTime.Time + new TimeStep(0, 2), TimeStep.zero, true);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                m_interactive.StartAt(RoomTime.Time - new TimeStep(0, 2), TimeStep.zero, true);
            }
            m_interactive.Update();
            m_view.Move(m_interactive.ViewSnapshot);
        }

    }

}
