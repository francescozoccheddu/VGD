using System.Collections.Generic;
using UnityEngine;
using Wheeled.Networking;

namespace Wheeled.Gameplay
{

    public sealed class PlayerHolder : MonoBehaviour
    {
        private class Test : InteractivePlayer.IFlushTarget
        {

            private int m_lastCount = -1;

            public void Flush(int _firstStep, IReadOnlyList<InputStep> _inputSteps, in Snapshot _snapshot)
            {
                if (_inputSteps.Count != m_lastCount)
                {
                    Debug.LogFormat("{0} steps (staring from {1})", _inputSteps.Count, _firstStep);
                    m_lastCount = _inputSteps.Count;
                }
            }
        }

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
            m_interactive.target = new Test();
            m_interactive.StartAt(RoomTime.Time, TimeStep.zero, false);
        }

        private void Update()
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
