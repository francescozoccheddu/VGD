using UnityEngine;

using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    public abstract class Offense
    {
        public byte OffenderId { get; }
        public Vector3 Origin { get; }

        public abstract EOffenseType Type { get; }

        public Offense(byte _offenderId, Vector3 _origin)
        {
            OffenderId = _offenderId;
            Origin = _origin;
        }
    }

    public sealed class ExplosionOffense : Offense
    {
        public override EOffenseType Type => EOffenseType.Explosion;

        public ExplosionOffense(byte _offenderId, Vector3 _position) : base(_offenderId, _position)
        {
        }
    }

    public abstract class ShotOffense : Offense
    {
        public Sight Sight { get; }

        public Vector3? Hit { get; set; }

        public ShotOffense(byte _offenderId, Vector3 _position, Sight _sight) : base(_offenderId, _position)
        {
            Sight = _sight;
            Hit = null;
        }
    }

    public sealed class RocketShotOffense : ShotOffense
    {
        public override EOffenseType Type => EOffenseType.Rocket;

        public const double c_maxLifetime = 5.0f;
        public const float c_velocity = 20.0f;

        public RocketShotOffense(byte _offenderId, Vector3 _position, Sight _sight) : base(_offenderId, _position, _sight)
        {
        }
    }

    public sealed class RifleShotOffense : ShotOffense
    {
        public override EOffenseType Type => EOffenseType.Rifle;
        public float Power { get; }

        public const float c_maxDistance = 100.0f;

        public RifleShotOffense(byte _offenderId, Vector3 _position, Sight _sight, float _power) : base(_offenderId, _position, _sight)
        {
            Power = _power;
        }
    }

    public struct DamageInfo
    {
        public byte offenderId;
        public int maxHealth;
        public int damage;
        public EOffenseType offenseType;
    }

    public struct KazeInfo
    {
        public Vector3 position;
    }

    public struct ShotInfo
    {
        public bool isRocket;
        public Vector3 position;
        public Sight sight;
    }

    public struct SpawnInfo
    {
        public int spawnPoint;
    }

    public struct KillInfo
    {
        public byte killerId;
        public byte victimId;
        public EOffenseType offenseType;
        public byte killerKills;
        public byte victimDeaths;
    }

    public enum EOffenseType
    {
        Rifle, Rocket, Explosion
    }
}