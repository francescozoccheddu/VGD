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

        private readonly PlayerController m_controller;

        private readonly OffenseBackstage m_offenseBackstage;

        public static TutorialPlayer Create() => new TutorialPlayer(new OffenseBackstage());

        public Snapshot Snapshot => this.GetSnapshot(LocalTime);

        public bool EnableSight { get => m_controller.EnableSight; set => m_controller.EnableSight = value; }
        public bool EnableMovement { get => m_controller.EnableMovement; set => m_controller.EnableMovement = value; }
        public bool EnableJump { get => m_controller.EnableJump; set => m_controller.EnableJump = value; }
        public bool EnableDash { get => m_controller.EnableDash; set => m_controller.EnableDash = value; }
        public bool EnableKaze { get => m_controller.EnableKaze; set => m_controller.EnableKaze = value; }
        public bool EnableRocket
        {
            get => m_controller.EnableRocket;
            set
            {
                m_view.EnableRocket = value;
                m_controller.EnableRocket = value;
            }
        }
        public bool EnableRifle
        {
            get => m_controller.EnableRifle; set
            {
                m_view.EnableRifle = value;
                m_controller.EnableRifle = value;
            }
        }

        public TutorialPlayer(OffenseBackstage _offenseBackstage) : base(0, _offenseBackstage, true)
        {

            m_offenseBackstage = _offenseBackstage;
            m_offenseBackstage.ValidationTarget = this;
            m_controller = new PlayerController(this)
            {
                EnableDash = false,
                EnableJump = false,
                EnableKaze = false,
                EnableMovement = false,
                EnableRifle = false,
                EnableRocket = false,
                EnableSight = false
            };
            EnableRifle = false;
            EnableRocket = false;
        }

        protected override void OnActorBreathed()
        {
            base.OnActorBreathed();
            m_controller.OnActorBreathed();
        }

        protected override void OnActorDied()
        {
            base.OnActorDied();
            m_controller.OnActorDied();
        }

        protected override void OnActorSpawned()
        {
            base.OnActorSpawned();
            m_controller.OnActorSpawned();
        }

        protected override void OnUpdated()
        {
            m_offenseBackstage.UpdateUntil(LocalTime);
            base.OnUpdated();
            m_controller.OnUpdated();
        }

        protected override void OnInfoSetup()
        {
            base.OnInfoSetup();
            m_controller.OnInfoSetup();
        }

        IEnumerable<OffenseBackstage.HitTarget> OffenseBackstage.IValidationTarget.ProvideHitTarget(double _time, Offense _offense) => Enumerable.Empty<OffenseBackstage.HitTarget>();
        void OffenseBackstage.IValidationTarget.Damage(double _time, int _offendedId, Offense _offense, float _damage) { }
        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense) => true;
    }
}