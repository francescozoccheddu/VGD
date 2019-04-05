using Wheeled.Gameplay.Action;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private abstract class Player : PlayerBase
        {
            protected Player(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
            }

            public void PutDeath(double _time, DeathInfo _info, int _deaths)
            {
                PutDeath(_time, _info);
                PutDeaths(_time, _deaths);
            }

            public new void PutShoot(double _time, ShotInfo _info)
            {
                base.PutShoot(_time, _info);
            }

            public new void PutSpawn(double _time, SpawnInfo _info)
            {
                base.PutSpawn(_time, _info);
            }
        }
    }
}