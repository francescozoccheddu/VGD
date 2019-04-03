using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        private sealed class LocalPlayer : Player, MovementController.ICommitTarget, ActionController.ITarget
        {
            private readonly ActionController m_actionController;
            private readonly MovementController m_movementController;

            public LocalPlayer(ServerGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_movementController = new MovementController()
                {
                    target = this
                };
                m_actionController = new ActionController()
                {
                    Target = this
                };
            }

            public void Start()
            {
                m_movementController.StartAt(m_manager.m_time);
            }

            public override void Update()
            {
                m_actionHistory.Update(m_manager.m_time);
                m_movementController.UpdateUntil(m_manager.m_time);
                UpdateView(m_movementController.ViewSnapshot);
                m_actionHistory.Perform();
                HandleRespawn();
                m_actionController.Update(m_actionHistory, m_movementController.ViewSnapshot);
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

            void ActionController.ITarget.Kaze()
            {
                m_actionHistory.PutDeath(m_manager.m_time, new DeathInfo
                {
                    isExploded = true,
                    killerId = Id,
                    offenseType = OffenseType.Kaze
                });
            }

            void ActionController.ITarget.Shoot(ShotInfo _info)
            {
                m_actionHistory.PutShot(m_manager.m_time, _info);
            }

            protected override void SendReplication()
            {
                m_manager.SendAll(NetworkManager.SendMethod.Unreliable);
            }
        }
    }
}