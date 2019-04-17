﻿using Wheeled.Gameplay;
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
                m_ShouldHandleRespawn = true;
            }

            public override bool IsLocal => true;

            public void Start()
            {
                m_movementController.StartAt(LocalTime);
            }

            #region MovementController.ICommitTarget

            void MovementController.ICommitTarget.Commit(int _step, InputStep _input, Snapshot _snapshot)
            {
                PutInput(_step, _input);
                PutSimulation(_step, _snapshot.simulation);
                PutSight(_step, _snapshot.sight);
            }

            #endregion MovementController.ICommitTarget

            #region ActionController.ITarget

            void ActionController.ITarget.Kaze(KazeInfo _info)
            {
                DeathInfo deathInfo = new DeathInfo
                {
                    isExploded = true,
                    killerId = Id,
                    offenseType = OffenseType.Explosion,
                    position = _info.position
                };
                PutDeath(m_manager.m_time, deathInfo);
            }

            void ActionController.ITarget.Shoot(ShotInfo _info)
            {
                PutShoot(m_manager.m_time, _info);
            }

            #endregion ActionController.ITarget

            protected override int GetLastValidMovementStep()
            {
                return LocalTime.SimulationSteps();
            }

            protected override void OnSpawn(double _time, Snapshot _snapshot)
            {
                m_movementController.Teleport(_snapshot, true);
            }

            protected override void OnUpdated()
            {
                if (ActionHistoryLocalTimeQuery.IsAlive)
                {
                    if (!m_movementController.IsRunning)
                    {
                        m_movementController.StartAt(LocalTime);
                    }
                }
                else
                {
                    m_movementController.Pause();
                }
                m_movementController.UpdateUntil(LocalTime);
                PutSight(m_movementController.Step, m_movementController.RawSnapshot.sight);
                m_actionController.Update(ActionHistoryLocalTimeQuery, GetSnapshot(LocalTime));
            }

            protected override void SendReplication(NetworkManager.SendMethod _method)
            {
                m_manager.SendAll(_method);
            }
        }
    }
}