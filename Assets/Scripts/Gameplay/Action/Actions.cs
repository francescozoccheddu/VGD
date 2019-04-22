using UnityEngine;

using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal abstract class Offense
    {
        #region Public Properties

        public byte OffenderId { get; }
        public Vector3 Origin { get; }

        public abstract OffenseType Type { get; }

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
        #region Public Properties

        public override OffenseType Type => OffenseType.Explosion;

        #endregion Public Properties

        #region Public Constructors

        public ExplosionOffense(byte _offenderId, Vector3 _position) : base(_offenderId, _position)
        {
        }

        #endregion Public Constructors
    }

    internal abstract class ShotOffense : Offense
    {
        #region Public Properties

        public Sight Sight { get; }

        public Vector3? Hit { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public ShotOffense(byte _offenderId, Vector3 _position, Sight _sight) : base(_offenderId, _position)
        {
            Sight = _sight;
            Hit = null;
        }

        #endregion Public Constructors
    }

    internal sealed class RocketShotOffense : ShotOffense
    {
        #region Public Properties

        public override OffenseType Type => OffenseType.Rocket;

        #endregion Public Properties

        #region Public Fields

        public const double c_maxLifetime = 5.0f;
        public const float c_velocity = 20.0f;

        #endregion Public Fields

        #region Public Constructors

        public RocketShotOffense(byte _offenderId, Vector3 _position, Sight _sight) : base(_offenderId, _position, _sight)
        {
        }

        #endregion Public Constructors
    }

    internal sealed class RifleShotOffense : ShotOffense
    {
        #region Public Properties

        public override OffenseType Type => OffenseType.Rifle;
        public float Power { get; }

        #endregion Public Properties

        #region Public Fields

        public const float c_maxDistance = 100.0f;

        #endregion Public Fields

        #region Public Constructors

        public RifleShotOffense(byte _offenderId, Vector3 _position, Sight _sight, float _power) : base(_offenderId, _position, _sight)
        {
            Power = _power;
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

        public int spawnPoint;

        #endregion Public Fields
    }

    internal struct KillInfo
    {
        #region Public Fields

        public byte killerId;
        public byte victimId;
        public OffenseType offenseType;
        public byte killerKills;
        public byte victimDeaths;

        #endregion Public Fields
    }

    internal enum OffenseType
    {
        Rifle, Rocket, Explosion
    }
}