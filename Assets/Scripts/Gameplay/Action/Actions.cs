using UnityEngine;

using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal enum OffenseType
    {
        Rifle, Rocket, Explosion
    }

    #region Infos

    internal struct DamageInfo
    {
        public byte offenderId;
        public int maxHealth;
        public int damage;
        public OffenseType offenseType;
    }

    internal struct KazeInfo
    {
        public Vector3 position;
    }

    internal struct DeathInfo
    {
        public Vector3 position;
        public bool isExploded;
        public byte killerId;
        public OffenseType offenseType;
    }

    internal struct HitConfirmInfo
    {
        public OffenseType offenseType;
    }

    internal struct ShotInfo
    {
        public bool isRocket;
        public Vector3 position;
        public Sight sight;
    }

    internal struct SpawnInfo
    {
        public byte spawnPoint;
    }

    #endregion

    #region Offense

    internal abstract class Offense
    {
        public byte OffenderId { get; }
        public Vector3 Origin { get; }

        public Offense(byte _offenderId, Vector3 _origin)
        {
            OffenderId = _offenderId;
            Origin = _origin;
        }
    }

    internal sealed class ExplosionOffense : Offense
    {
        public ExplosionOffense(byte _offenderId, Vector3 _position) : base(_offenderId, _position)
        {
        }
    }

    internal abstract class ShotOffense : Offense
    {
        public ShotOffense(byte _offenderId, Vector3 _position, Vector3 _direction) : base(_offenderId, _position)
        {
            Direction = _direction;
            HitDistance = null;
        }

        public Vector3 Direction { get; }
        public float? HitDistance { get; set; }

    }

    internal sealed class RocketShotOffense : ShotOffense
    {
        public RocketShotOffense(byte _offenderId, Vector3 _position, Vector3 _direction) : base(_offenderId, _position, _direction)
        {
        }
    }

    internal sealed class RifleShotOffense : ShotOffense
    {
        public RifleShotOffense(byte _offenderId, Vector3 _position, Vector3 _direction, float _power) : base(_offenderId, _position, _direction)
        {
        }

        public float Power { get; }

    }

    #endregion

}