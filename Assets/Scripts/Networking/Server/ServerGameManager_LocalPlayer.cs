using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        private sealed class LocalPlayer : Player, MovementController.ICommitTarget
        {
            private readonly MovementController m_movementController;

            public LocalPlayer(ServerGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_movementController = new MovementController()
                {
                    target = this
                };
            }

            public void Start()
            {
                m_movementController.StartAt(m_manager.m_time);
            }

            public override void Update()
            {
                m_movementController.UpdateUntil(m_manager.m_time);
                UpdateView(m_manager.m_time, m_movementController.ViewSnapshot);
                HandleRespawn();
                Trim();
            }

            void MovementController.ICommitTarget.Commit(int _step, InputStep _input, Snapshot _snapshot)
            {
                m_inputHistory.Put(_step, _input);
                PutSimulation(_step, _snapshot.simulation);
                PutSight(_step, _snapshot.sight);
            }

            void MovementController.ICommitTarget.Cut(int _oldest)
            {
                m_inputHistory.Cut(_oldest);
            }

            protected override void SendReplication()
            {
                m_manager.SendAll(NetworkManager.SendMethod.Unreliable);
            }
        }
    }
}