﻿namespace Wheeled.Networking
{
    internal struct PlayerRecapInfo
    {
        #region Public Fields

        public int deaths;
        public int health;
        public byte id;
        public int kills;
        public byte ping;

        #endregion Public Fields
    }

    // Notify: Client tells Server
    // Replication: Server tells Client about someone else
    // Order: Server tells Client about itself
    // Sync: Server tells Client about the room
    internal enum Message
    {
        // Movement
        MovementNotify, SimulationOrder, MovementReplication,

        // Room
        TimeSync, ReadyNotify, PlayerIntroductionSync, PlayerWelcomeSync, RecapSync, QuitReplication,

        // Actions
        KazeNotify, ShootNotify, DeathOrderOrReplication, SpawnOrderOrReplication, ShootReplication, DamageOrderOrReplication
    }
}