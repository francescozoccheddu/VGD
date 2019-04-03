using UnityEngine;

using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal enum OffenseType
    {
        Rifle, Rocket, Kaze
    }

    internal struct DamageInfo
    {
        public Vector3 offensePosition;
        public OffenseType offenseType;
    }

    internal struct DeathInfo
    {
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
        public int spawnPoint;
    }
}