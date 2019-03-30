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

    internal struct KazeInfo
    {
        public Vector3 position;
    }

    internal struct GunShotInfo : IFarce
    {
        public byte id;
        public Vector3 position;
        public Sight sight;
        public bool rocket;
        public float power;
    }

    internal struct DeathInfo : IFarce
    {
        public byte deadId;
        public byte killerId;
        public DeathCause cause;
        public Vector3 position;
        public bool explosion;
    }

    internal struct SpawnInfo : IFarce
    {
        public int spawnPoint;
    }

    internal struct LocalDamageInfo : IFarce
    {
        public Vector3 offensePosition;
    }

    internal struct LocalHitConfirmInfo : IFarce
    {

    }

    internal struct GunHitInfo : IFarce
    {
        public byte id;
        public Vector3 position;
        public Vector3 normal;
    }

}
