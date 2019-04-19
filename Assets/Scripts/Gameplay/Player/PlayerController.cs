using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Networking;

namespace Wheeled.Gameplay.Player
{
    internal abstract class LocalPlayer : MovementController.ICommitTarget, ActionController.ITarget, EventHistory<DamageInfo>.ITarget, EventHistory<HitConfirmInfo>.ITarget
    {
        #region Private Fields

        private readonly ActionController m_actionController;
        private readonly MovementController m_movementController;
        private readonly EventHistory<DamageInfo> m_damageHistory;
        private readonly EventHistory<HitConfirmInfo> m_hitConfirmHistory;
        private readonly PlayerBase m_player;

        #endregion Private Fields

        #region Protected Constructors

        protected LocalPlayer(PlayerBase _player)
        {
            m_player = _player;
            m_actionController = new ActionController();
            m_movementController = new MovementController();
            m_damageHistory = new EventHistory<DamageInfo>
            {
                Target = this
            };
            m_hitConfirmHistory = new EventHistory<HitConfirmInfo>
            {
                Target = this
            };
        }

        #endregion Protected Constructors

        #region Public Methods

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

        void EventHistory<HitConfirmInfo>.ITarget.Perform(double _time, HitConfirmInfo _value)
        {
        }

        void EventHistory<DamageInfo>.ITarget.Perform(double _time, DamageInfo _value)
        {
        }

        public void OnUpdated(IReadOnlyPlayer _player)
        {
            m_actionController.Update(_player.LocalTime, _player);
        }

        public void OnActorDied()
        {
            m_movementController.Pause();
        }

        public void OnDamageScheduled(double _time, DamageInfo _info)
        {
            m_damageHistory.Put(_time, _info);
        }

        public void OnActorSpawned()
        {
            m_movementController.Teleport(m_player.GetSnapshot(m_player.LocalTime), true);
            m_movementController.StartAt(m_player.LocalTime);
        }

        public void OnActorBreathed()
        {
            m_movementController.UpdateUntil(m_player.LocalTime);
            m_player.PutSight(m_movementController.Step, m_movementController.RawSnapshot.sight);
        }

        #endregion Public Methods
    }
}