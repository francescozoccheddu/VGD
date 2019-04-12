using UnityEngine;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionController
    {
        public interface ITarget
        {
            void Kaze(KazeInfo _info);

            void Shoot(ShotInfo _info);
        }

        public ITarget Target { get; set; }

        public void Update(ActionHistory.StaticQuery _query, in Snapshot _snapshot)
        {
            if (Target != null)
            {
                if (Input.GetButtonDown("ShootRifle") && _query.CanShootRifle)
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = _snapshot.simulation.Position,
                        sight = _snapshot.sight,
                        isRocket = false
                    });
                }
                if (Input.GetButtonDown("ShootRocket") && _query.CanShootRocket)
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = _snapshot.simulation.Position,
                        sight = _snapshot.sight,
                        isRocket = true
                    });
                }
                if (Input.GetButtonDown("Kaze") && _query.CanKaze)
                {
                    Target.Kaze(new KazeInfo
                    {
                        position = _snapshot.simulation.Position
                    });
                }
            }
        }
    }
}