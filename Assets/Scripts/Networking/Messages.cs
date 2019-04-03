namespace Wheeled.Networking
{
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
        KazeNotify, ShootNotify, DeathOrderOrReplication, SpawnOrderOrReplication, ShootReplication, DamageOrder, HitConfirmOrder
    }

    internal struct PlayerRecapInfo
    {
        public int deaths;
        public byte health;
        public byte id;
        public int kills;
        public byte ping;
    }
}