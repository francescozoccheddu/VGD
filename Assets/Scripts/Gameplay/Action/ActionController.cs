using UnityEngine;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionController
    {

        private readonly ActionHistory m_actionHistory;

        public ActionController(ActionHistory _actionHistory)
        {
            m_actionHistory = _actionHistory;
        }

        public ITarget Target { get; set; }

        public interface ITarget
        {
            void Kaze();

            void ShootRifle(float _power);

            void ShootRocket();
        }

        public void Update()
        {
            if (Target != null)
            {
                if (Input.GetButtonDown("ShootRifle") && m_actionHistory.CanShootRifle)
                {
                    Target.ShootRifle(m_actionHistory.RiflePower);
                }
                if (Input.GetButtonDown("ShootRocket") && m_actionHistory.CanShootRocket)
                {
                    Target.ShootRocket();
                }
                if (Input.GetButtonDown("Kaze") && m_actionHistory.CanKaze)
                {
                    Target.Kaze();
                }
            }
        }
    }
}