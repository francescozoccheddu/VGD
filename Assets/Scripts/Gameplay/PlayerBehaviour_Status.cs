using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour
    {

        private sealed class ActionHistory
        {

            public interface Action
            {
                void Run();
            }

            public struct DieAction : Action
            {
                public Vector3 hitDirection;
                public Vector3 hitPoint;
                public bool exploded;

                public void Run()
                {
                }
            }

            public struct KazeAction : Action
            {
                public float intensity;

                public void Run()
                {
                }
            }

            public struct SpawnAction : Action
            {
                public int spawnPoint;

                public void Run()
                {
                }
            }

            public struct ShootAction : Action
            {

                public Vector3 direction;
                public WeaponType weapon;
                public float intensity;

                public void Run()
                {
                }
            }

            public void Add(Time _time, Action _action)
            {
            }

            public void Run(Time _time)
            {
            }

        }

        private sealed class StatusHistory
        {

            public enum Status
            {
                Dead, Alive, Unborn
            }

            public void Add(Time _time, Status _status)
            {

            }

            public Status Get(Time _time)
            {
                throw new System.Exception();
            }

            public void Forget(Time _time)
            {

            }

        }

        private readonly ActionHistory m_actionHistory = new ActionHistory();
        private readonly StatusHistory m_simulationState = new StatusHistory();

        public void UpdateStatus()
        {
            m_actionHistory.Run(m_presentationTime);
        }

    }
}