using Wheeled.Core;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.Player;

namespace Wheeled.Tutorial
{
    public sealed class TutorialPlayer : Player
    {

        public PlayerController Controller { get; }

        private readonly OffenseBackstage m_offenseBackstage;

        public static TutorialPlayer Create() => new TutorialPlayer(new OffenseBackstage());

        public Snapshot Snapshot => this.GetSnapshot(LocalTime);

        public TutorialPlayer(OffenseBackstage _offenseBackstage) : base(0, _offenseBackstage, true)
        {

            m_offenseBackstage = _offenseBackstage;
            Controller = new PlayerController(this)
            {
                EnableDash = false,
                EnableJump = false,
                EnableKaze = false,
                EnableMovement = false,
                EnableRifle = false,
                EnableRocket = false,
                EnableSight = false
            };
        }

        protected override void OnActorBreathed()
        {
            base.OnActorBreathed();
            Controller.OnActorBreathed();
        }

        protected override void OnActorDied()
        {
            base.OnActorDied();
            Controller.OnActorDied();
        }

        protected override void OnActorSpawned()
        {
            base.OnActorSpawned();
            Controller.OnActorSpawned();
        }

        protected override void OnUpdated()
        {
            base.OnUpdated();
            m_offenseBackstage.UpdateUntil(LocalTime);
            Controller.OnUpdated();
        }

        protected override void OnInfoSetup()
        {
            base.OnInfoSetup();
            Controller.OnInfoSetup();
        }
    }
}