using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Scene;
using Wheeled.HUD;

namespace Wheeled.Gameplay.Player
{
    internal sealed class PlayerController : MovementController.ICommitTarget, ActionController.ITarget, EventHistory<DamageInfo>.ITarget, EventHistory<OffenseType>.ITarget
    {
        #region Public Properties

        public int MovementStep => m_movementController.Step;

        #endregion Public Properties

        #region Private Fields

        private readonly ActionController m_actionController;
        private readonly MovementController m_movementController;
        private readonly EventHistory<DamageInfo> m_damageHistory;
        private readonly EventHistory<OffenseType> m_hitConfirmHistory;
        private readonly Player m_player;

        #endregion Private Fields

        #region Public Constructors

        public PlayerController(Player _player)
        {
            m_player = _player;
            m_actionController = new ActionController
            {
                Target = this
            };
            m_movementController = new MovementController
            {
                target = this
            };
            m_damageHistory = new EventHistory<DamageInfo>
            {
                Target = this
            };
            m_hitConfirmHistory = new EventHistory<OffenseType>
            {
                Target = this
            };
            DeathCamera.EnableDefault();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Teleport(CharacterController _snapshot)
        {
            m_movementController.Teleport(new Snapshot
            {
                sight = m_movementController.RawSnapshot.sight,
                simulation = _snapshot
            }, false);
        }

        public void PutHitConfirm(double _time, OffenseType _info)
        {
            m_hitConfirmHistory.Put(_time, _info);
        }

        void ActionController.ITarget.Kaze(KazeInfo _info)
        {
            m_player.PutKaze(m_player.LocalTime, _info);
        }

        void ActionController.ITarget.Shoot(ShotInfo _info)
        {
            m_player.PutShot(m_player.LocalTime, _info);
        }

        void MovementController.ICommitTarget.Commit(int _step, InputStep _input, Snapshot _snapshot)
        {
            m_player.PutSimulation(_step, _snapshot.simulation);
            m_player.PutSight(_step, _snapshot.sight);
            m_player.PutInput(_step, _input);
        }

        void EventHistory<OffenseType>.ITarget.Perform(double _time, OffenseType _value)
        {
        }

        void EventHistory<DamageInfo>.ITarget.Perform(double _time, DamageInfo _value)
        {
        }

        public void OnUpdated()
        {
            m_actionController.Update(m_player.LocalTime, m_player);
            m_hitConfirmHistory.PerformUntil(m_player.LocalTime);
            m_damageHistory.PerformUntil(m_player.LocalTime);
        }

        public void OnActorDied()
        {
            m_movementController.Pause();
            CrossHairBehaviour.SetEnabled(false);
            DeathCamera.Enable(m_player.GetSnapshot(m_player.LocalTime).simulation.Position);
        }

        public void OnDamageScheduled(double _time, DamageInfo _info)
        {
            m_damageHistory.Put(_time, _info);
        }

        public void OnActorSpawned()
        {
            m_movementController.Teleport(m_player.GetSnapshot(m_player.LocalTime), true);
            m_movementController.StartAt(m_player.LocalTime);
            CrossHairBehaviour.SetEnabled(true);
            DeathCamera.Disable();
        }

        public void OnActorBreathed()
        {
            CrossHairBehaviour.SetHealth(m_player.LifeHistory.GetHealth(m_player.LocalTime));
            m_movementController.UpdateUntil(m_player.LocalTime);
            m_player.PutSight(m_movementController.Step, m_movementController.RawSnapshot.sight);
        }

        #endregion Public Methods
    }
}