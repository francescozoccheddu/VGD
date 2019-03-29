using UnityEngine;

namespace Wheeled.Gameplay.Action
{

    internal interface IAction { }

    internal interface IHealthAction : IAction
    {
        int Health { get; }
    }

    internal struct HitAction : IHealthAction
    {
        public Vector3 offensePosition;
        public int Health { get; }
    }

    internal struct ShotFeedbackAction : IAction
    {
        public bool hit;
        public bool rocket;
        public Vector3 position;
        public Vector3 direction;
    }

    internal struct KillAction : IAction
    {

    }

    internal struct SpawnAction : IHealthAction
    {
        public const int c_spawnHealth = 100;
        public byte spawnPoint;
        public int Health => c_spawnHealth;
    }

    internal struct RocketShootAction : IAction
    {
        public int id;
        public Vector3 position;
        public float turn;
        public float lookUp;
    }

    internal struct RifleShootAction : IAction
    {
        public int id;
        public Vector3 position;
        public float turn;
        public float lookUp;
        public float power;
    }

    internal struct ShotInfo
    {
        public bool rocket;
        public Vector3 position;
        public Vector3 direction;
    }

    internal struct DeathInfo
    {
        public bool exploded;
    }

    internal struct SpawnInfo
    {
        public byte spawnPoint;
    }

    internal struct HitInfo
    {
        public byte shotId;
        public Vector3 offensePosition;
    }

    internal struct LifeUpdateAction : IHealthAction
    {
        public int Health { get; set; }
    }

    internal struct PlayerStatus
    {

        public int deaths;
        public int kills;
    }

}
