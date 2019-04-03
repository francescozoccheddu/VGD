using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionController
    {
        public interface ITarget
        {
            void Kaze();

            void Shoot(ShotInfo _info);
        }

        public ITarget Target { get; set; }

        public void Update(ActionHistory _actionHistory, in Snapshot _snapshot)
        {
            if (Target != null)
            {
                if (Input.GetButtonDown("ShootRifle") && _actionHistory.CanShootRifle)
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = _snapshot.simulation.position,
                        sight = _snapshot.sight,
                        isRocket = false
                    });
                }
                if (Input.GetButtonDown("ShootRocket") && _actionHistory.CanShootRocket)
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = _snapshot.simulation.position,
                        sight = _snapshot.sight,
                        isRocket = true
                    });
                }
                if (Input.GetButtonDown("Kaze") && _actionHistory.CanKaze)
                {
                    Target.Kaze();
                }
            }
        }
    }
}