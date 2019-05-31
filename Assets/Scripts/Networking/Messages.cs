namespace Wheeled.Networking
{
    public struct PlayerRecapInfo
    {
        public int deaths;
        public int health;
        public int id;
        public int kills;
        public int ping;
    }

    public enum EMessage
    {
        // Movement
        MovementNotify, SimulationOrder, MovementReplication,

        // Room
        TimeSync, ReadyNotify, PlayerIntroductionSync, PlayerWelcomeSync, RecapSync, QuitReplication,

        // Actions
        KazeNotify, ShootNotify, KillSync, SpawnOrderOrReplication, ShootReplication, DamageOrderOrReplication
    }
}