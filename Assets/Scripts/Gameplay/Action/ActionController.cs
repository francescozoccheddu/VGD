using UnityEngine;
using Wheeled.Gameplay.Movement;
using Wheeled.Networking;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionController
    {
        #region Public Interfaces

        public interface ITarget
        {
            #region Public Methods

            void Kaze(KazeInfo _info);

            void Shoot(ShotInfo _info);

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Public Properties

        public ITarget Target { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Update(double _time, IReadOnlyPlayer _player)
        {
            int? health = null;
            int GetHealth()
            {
                if (health == null)
                {
                    health = _player.LifeHistory.GetHealth(_time);
                }
                return health.Value;
            }
            Snapshot? snapshot = null;
            Snapshot GetSnapshot()
            {
                if (snapshot == null)
                {
                    snapshot = _player.GetSnapshot(_time);
                }
                return snapshot.Value;
            }
            if (Target != null)
            {
                if (Input.GetButtonDown("ShootRifle") && LifeHistoryHelper.IsAlive(GetHealth()) && _player.WeaponsHistory.CanShootRifle(_time, out _))
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = GetSnapshot().simulation.Position,
                        sight = GetSnapshot().sight,
                        isRocket = false
                    });
                }
                if (Input.GetButtonDown("ShootRocket") && LifeHistoryHelper.IsAlive(GetHealth()) && _player.WeaponsHistory.CanShootRocket(_time))
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = GetSnapshot().simulation.Position,
                        sight = GetSnapshot().sight,
                        isRocket = true
                    });
                }
                if (Input.GetButtonDown("Kaze") && !LifeHistoryHelper.IsExploded(GetHealth()))
                {
                    Target.Kaze(new KazeInfo
                    {
                        position = GetSnapshot().simulation.Position
                    });
                }
            }
        }

        #endregion Public Methods
    }
}