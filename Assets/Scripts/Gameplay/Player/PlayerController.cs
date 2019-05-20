using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Scene;
using Wheeled.UI.HUD;
using Wheeled.UI.HUD.DamageMarker;

namespace Wheeled.Gameplay.Player
{
    public sealed class PlayerController : MovementController.ICommitTarget, ActionController.ITarget, EventHistory<PlayerController.DamagePerformInfo>.ITarget, EventHistory<EOffenseType>.ITarget
    {
        public int MovementStep => m_movementController.Step;

        private readonly ActionController m_actionController;
        private readonly MovementController m_movementController;
        private readonly EventHistory<DamagePerformInfo> m_damageHistory;
        private readonly EventHistory<EOffenseType> m_hitConfirmHistory;
        private readonly Player m_player;

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
            m_damageHistory = new EventHistory<DamagePerformInfo>
            {
                Target = this
            };
            m_hitConfirmHistory = new EventHistory<EOffenseType>
            {
                Target = this
            };
            DeathCameraManager.EnableDefault();
        }

        struct DamagePerformInfo
        {
            public DamageInfo info;
            public Vector3? origin;
        }

        public void Teleport(CharacterController _snapshot)
        {
            m_movementController.Teleport(new Snapshot
            {
                sight = m_movementController.RawSnapshot.sight,
                simulation = _snapshot
            }, false);
        }

        public void PutHitConfirm(double _time, EOffenseType _info)
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

        public void OnInfoSetup()
        {
            Color color = Scripts.PlayerPreferences.colors[m_player.Info.Value.color];
            InGameHUDBehaviour.Instance.leftCrossHair.Color = InGameHUDBehaviour.Instance.rightCrossHair.Color = color;
        }

        void EventHistory<EOffenseType>.ITarget.Perform(double _time, EOffenseType _value)
        {
            InGameHUDBehaviour.Instance.hitMarker.Hit();
        }

        void EventHistory<DamagePerformInfo>.ITarget.Perform(double _time, DamagePerformInfo _value)
        {
            InGameHUDBehaviour.Instance.healthIndicator.NotifyDamage();
            if (_value.origin != null && _value.info.offenderId != m_player.Id)
            {
                DamageMarkerManagerBehaviour.Instance.Add(_value.origin.Value);
            }
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
            InGameHUDBehaviour.Instance.SetAlive(false);
            DeathCameraManager.Enable(m_player.GetSnapshot(m_player.LocalTime).simulation.Position);
        }

        public void OnDamageScheduled(double _time, DamageInfo _info, Vector3? _offenderPosition)
        {
            m_damageHistory.Put(_time, new DamagePerformInfo { info = _info, origin = _offenderPosition });
        }

        public void OnActorSpawned()
        {
            m_movementController.Teleport(m_player.GetSnapshot(m_player.LocalTime), true);
            m_movementController.StartAt(m_player.LocalTime);
            InGameHUDBehaviour.Instance.SetAlive(true);
            DeathCameraManager.Disable();
        }

        public void OnActorBreathed()
        {
            InGameHUDBehaviour.Instance.healthIndicator.Health = m_player.LifeHistory.GetHealth(m_player.LocalTime);
            InGameHUDBehaviour.Instance.leftCrossHair.IsEnabled = m_player.WeaponsHistory.CanShootRifle(m_player.LocalTime, out _);
            InGameHUDBehaviour.Instance.rightCrossHair.IsEnabled = m_player.WeaponsHistory.CanShootRocket(m_player.LocalTime);
            m_movementController.UpdateUntil(m_player.LocalTime);
            m_player.PutSight(m_movementController.Step, m_movementController.RawSnapshot.sight);
        }
    }
}