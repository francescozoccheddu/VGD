using UnityEngine;

using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal enum DeathCause
    {
        Rifle, Rocket, Kaze
    }

    internal interface IFarce
    {
    }

    internal struct DeathInfo : IFarce
    {
        public DeathCause cause;
        public byte deadId;
        public bool explosion;
        public byte killerId;
        public Vector3 position;
    }

    internal struct GunHitInfo : IFarce
    {
        public byte id;
        public Vector3 normal;
        public Vector3 position;
    }

    internal struct GunShotInfo : IFarce
    {
        public byte id;
        public Vector3 position;
        public float power;
        public bool rocket;
        public Sight sight;
    }

    internal struct KazeInfo
    {
        public Vector3 position;
    }

    internal struct LocalDamageInfo : IFarce
    {
        public Vector3 offensePosition;
    }

    internal struct LocalHitConfirmInfo : IFarce
    {
    }

    internal struct SpawnInfo : IFarce
    {
        public int spawnPoint;
    }
}