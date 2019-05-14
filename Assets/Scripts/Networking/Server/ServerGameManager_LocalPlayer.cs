using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        #region Private Classes

        private sealed class LocalPlayer : AuthoritativePlayer
        {
            #region Public Properties

            public override bool IsLocal => true;

            #endregion Public Properties

            #region Private Fields

            private readonly PlayerController m_playerController;

            #endregion Private Fields

            #region Public Constructors

            public LocalPlayer(ServerGameManager _manager, byte _id, OffenseBackstage _offenseBackstage) : base(_manager, _id, _offenseBackstage) => m_playerController = new PlayerController(this);

            #endregion Public Constructors

            #region Public Methods

            public void PutHitConfirm(double _time, OffenseType _type) => m_playerController.PutHitConfirm(_time, _type);

            #endregion Public Methods

            #region Protected Methods

            protected override int GetLastValidMovementStep() => LocalTime.SimulationSteps();

            protected override void OnActorSpawned()
            {
                base.OnActorSpawned();
                m_playerController.OnActorSpawned();
            }

            protected override void OnActorDied()
            {
                base.OnActorDied();
                m_playerController.OnActorDied();
            }

            protected override void OnUpdated()
            {
                base.OnUpdated();
                m_playerController.OnUpdated();
            }

            protected override void OnDamageScheduled(double _time, DamageInfo _info)
            {
                base.OnDamageScheduled(_time, _info);
                Vector3? position = m_manager.GetPlayerById(_info.offenderId)?.GetSnapshot(_time).simulation.Position;
                m_playerController.OnDamageScheduled(_time, _info, position);
            }

            protected override void OnActorBreathed()
            {
                base.OnActorBreathed();
                m_playerController.OnActorBreathed();
            }

            protected override void SendReplication(NetworkManager.SendMethod _method) => m_manager.SendAll(_method);

            protected override void OnInfoSetup()
            {
                base.OnInfoSetup();
                m_playerController.OnInfoSetup();
            }

            #endregion Protected Methods
        }

        #endregion Private Classes
    }
}