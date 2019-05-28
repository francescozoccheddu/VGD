using UnityEngine;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;

namespace Wheeled.Gameplay.Action
{
    public sealed class ActionController
    {

        public bool EnableKaze { get; set; } = true;
        public bool EnableRifle { get; set; } = true;
        public bool EnableRocket { get; set; } = true;

        public interface ITarget
        {
            void Kaze(KazeInfo _info);

            void Shoot(ShotInfo _info);
        }

        public ITarget Target { get; set; }

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
                if (EnableRifle && Input.GetButtonDown("ShootRifle") && LifeHistoryHelper.IsAlive(GetHealth()) && _player.WeaponsHistory.CanShootRifle(_time, out _))
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = GetSnapshot().simulation.Position,
                        sight = GetSnapshot().sight,
                        isRocket = false
                    });
                }
                if (EnableRocket && Input.GetButtonDown("ShootRocket") && LifeHistoryHelper.IsAlive(GetHealth()) && _player.WeaponsHistory.CanShootRocket(_time))
                {
                    Target.Shoot(new ShotInfo
                    {
                        position = GetSnapshot().simulation.Position,
                        sight = GetSnapshot().sight,
                        isRocket = true
                    });
                }
                if (EnableKaze && Input.GetButtonDown("Kaze") && !LifeHistoryHelper.IsExploded(GetHealth()))
                {
                    _player.LifeHistory.GetLastDeathInfo(_time, out DamageNode? death, out DamageNode? explosion);
                    if (explosion == null && (_time - death?.time > ActionValidator.c_maxKazeWaitAfterDeath != true))
                    {
                        Target.Kaze(new KazeInfo
                        {
                            position = GetSnapshot().simulation.Position
                        });
                    }
                }
            }
        }
    }
}