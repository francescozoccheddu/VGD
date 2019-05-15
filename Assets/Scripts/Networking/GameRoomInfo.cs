using System.Net;

namespace Wheeled.Networking
{
    public delegate void GameRoomDiscoverEventHandler(GameRoomInfo _room);

    public struct GameRoomInfo
    {
        public IPEndPoint endPoint;
        public byte map;
    }
}