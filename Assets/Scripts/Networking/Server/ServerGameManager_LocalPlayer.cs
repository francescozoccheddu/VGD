using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Player;

namespace Wheeled.Networking.Server
{
    internal sealed partial class ServerGameManager
    {
        #region Private Classes

        private sealed class LocalPlayer : Player
        {
            #region Public Properties

            public override bool IsLocal => true;

            #endregion Public Properties

            #region Private Fields

            private readonly PlayerController m_playerController;

            #endregion Private Fields

            #region Public Constructors

            public LocalPlayer(ServerGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_playerController = new PlayerController(this);
            }

            #endregion Public Constructors

            #region Protected Methods

            protected override int GetLastValidMovementStep()
            {
                return LocalTime.SimulationSteps();
            }

            protected override void OnActorSpawned()
            {
                base.OnActorSpawned();
                m_playerController.OnActorSpawned();
            }

            protected override void OnUpdated()
            {
                base.OnUpdated();
                m_playerController.OnUpdated();
            }

            protected override void OnDamageScheduled(double _time, DamageInfo _info)
            {
                base.OnDamageScheduled(_time, _info);
                m_playerController.OnDamageScheduled(_time, _info);
            }

            protected override void OnActorBreathed()
            {
                base.OnActorBreathed();
                m_playerController.OnActorBreathed();
            }

            protected override void SendReplication(NetworkManager.SendMethod _method)
            {
                m_manager.SendAll(_method);
            }

            #endregion Protected Methods
        }

        #endregion Private Classes
    }
}