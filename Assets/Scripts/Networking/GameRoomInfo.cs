using System.Net;

namespace Wheeled.Networking
{

    public delegate void GameRoomDiscoverEventHandler(GameRoomInfo _room);

    public readonly struct GameRoomInfo
    {

        public readonly IPEndPoint endPoint;
        public readonly string name;
        public readonly byte map;

        public GameRoomInfo(IPEndPoint _endPoint, string _name, byte _map)
        {
            endPoint = _endPoint;
            name = _name;
            map = _map;
        }
    }

}
