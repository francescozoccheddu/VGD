using UnityEngine;

namespace Wheeled.Gameplay.Action
{

    internal static class ActionController
    {

        public interface ITarget
        {

            void ShootRifle(float _power);

            void ShootRocket();

            void Kaze();

        }

        public static void Process(ActionHistory _actionHistory, ITarget _target)
        {
            if (Input.GetButtonDown("ShootRifle"))
            {
                if (_actionHistory.CanShootRifle)
                {
                    _target.ShootRifle(_actionHistory.RiflePower);
                }
            }
            if (Input.GetButtonDown("ShootRocket"))
            {
                if (_actionHistory.CanShootRocket)
                {
                    _target.ShootRocket();
                }
            }
            if (Input.GetButtonDown("Kaze"))
            {
            }
        }

    }

}
