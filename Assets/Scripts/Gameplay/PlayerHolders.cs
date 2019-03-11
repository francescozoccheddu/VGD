using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay
{

    internal static class PlayerHolders
    {

        public static InteractivePlayerHolder SpawnPlayerHolder()
        {
            InteractivePlayerHolder playerHolder = new InteractivePlayerHolder();
            new UpdatableHolder(playerHolder)
            {
                IsRunning = true
            };
            return playerHolder;
        }

        public sealed class InteractivePlayerHolder : IUpdatable
        {

            public readonly MovementController m_interactive;
            public readonly PlayerView m_view;

            public InteractivePlayerHolder()
            {
                m_interactive = new MovementController();
                m_view = new PlayerView();
            }

            void IUpdatable.Update()
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    m_interactive.Teleport(new Snapshot(), false);
                    Debug.Log("Teleport");
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    m_interactive.StartAt(RoomTime.Time, TimeStep.zero, false);
                    Debug.Log("StartAt (Now)");
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    m_interactive.Pause(true);
                    Debug.Log("Pause");
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    m_interactive.StartAt(RoomTime.Time + new TimeStep(0, 2), TimeStep.zero, true);
                    Debug.Log("StartAt (Later)");
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    m_interactive.StartAt(RoomTime.Time - new TimeStep(0, 2), TimeStep.zero, true);
                    Debug.Log("StartAt (Sooner)");
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    m_interactive.FlushRate++;
                    Debug.LogFormat("FlushRate={0}", m_interactive.FlushRate);
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    m_interactive.FlushRate--;
                    Debug.LogFormat("FlushRate={0}", m_interactive.FlushRate);
                }
                m_interactive.Update();
                m_view.Move(m_interactive.ViewSnapshot);
            }

        }

    }

}

