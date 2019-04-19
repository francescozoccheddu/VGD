using UnityEngine;

using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal abstract class Offense
    {
        #region Public Properties

        public byte OffenderId { get; }
        public Vector3 Origin { get; }

        #endregion Public Properties

        #region Public Constructors

        public Offense(byte _offenderId, Vector3 _origin)
        {
            OffenderId = _offenderId;
            Origin = _origin;
        }

        #endregion Public Constructors
    }

    internal sealed class ExplosionOffense : Offense
    {
        #region Public Constructors

        public ExplosionOffense(byte _offenderId, Vector3 _position) : base(_offenderId, _position)
        {
        }

        #endregion Public Constructors
    }

    internal abstract class ShotOffense : Offense
    {
        #region Public Properties

        public Vector3 Direction { get; }

        public float? HitDistance { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public ShotOffense(byte _offenderId, Vector3 _position, Vector3 _direction) : base(_offenderId, _position)
        {
            Direction = _direction;
            HitDistance = null;
        }

        #endregion Public Constructors
    }

    internal sealed class RocketShotOffense : ShotOffense
    {
        #region Public Constructors

        public RocketShotOffense(byte _offenderId, Vector3 _position, Vector3 _direction) : base(_offenderId, _position, _direction)
        {
        }

        #endregion Public Constructors
    }

    internal sealed class RifleShotOffense : ShotOffense
    {
        #region Public Properties

        public float Power { get; }

        #endregion Public Properties

        #region Public Constructors

        public RifleShotOffense(byte _offenderId, Vector3 _position, Vector3 _direction, float _power) : base(_offenderId, _position, _direction)
        {
        }

        #endregion Public Constructors
    }

    internal struct DamageInfo
    {
        #region Public Fields

        public byte offenderId;
        public int maxHealth;
        public int damage;
        public OffenseType offenseType;

        #endregion Public Fields
    }

    internal struct KazeInfo
    {
        #region Public Fields

        public Vector3 position;

        #endregion Public Fields
    }

    internal struct ShotInfo
    {
        #region Public Fields

        public bool isRocket;
        public Vector3 position;
        public Sight sight;

        #endregion Public Fields
    }

    internal struct SpawnInfo
    {
        #region Public Fields

        public byte spawnPoint;

        #endregion Public Fields
    }

    internal enum OffenseType
    {
        Rifle, Rocket, Explosion
    }
}