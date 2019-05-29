using System.Collections.Generic;
using System.Linq;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.Player;

namespace Wheeled.Tutorial
{
    public sealed class TutorialPlayer : Player, OffenseBackstage.IValidationTarget
    {

        public PlayerController Controller { get; }

        private readonly OffenseBackstage m_offenseBackstage;

        public static TutorialPlayer Create() => new TutorialPlayer(new OffenseBackstage());

        public Snapshot Snapshot => this.GetSnapshot(LocalTime);

        public TutorialPlayer(OffenseBackstage _offenseBackstage) : base(0, _offenseBackstage, true)
        {

            m_offenseBackstage = _offenseBackstage;
            m_offenseBackstage.ValidationTarget = this;
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
            m_offenseBackstage.UpdateUntil(LocalTime);
            base.OnUpdated();
            Controller.OnUpdated();
        }

        protected override void OnInfoSetup()
        {
            base.OnInfoSetup();
            Controller.OnInfoSetup();
        }

        IEnumerable<OffenseBackstage.HitTarget> OffenseBackstage.IValidationTarget.ProvideHitTarget(double _time, Offense _offense) => Enumerable.Empty<OffenseBackstage.HitTarget>();
        void OffenseBackstage.IValidationTarget.Damage(double _time, int _offendedId, Offense _offense, float _damage) { }
        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense) => true;
    }
}