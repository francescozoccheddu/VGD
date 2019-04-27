using System.Net;

namespace Wheeled.Networking
{
    public delegate void GameRoomDiscoverEventHandler(GameRoomInfo _room);

    public struct GameRoomInfo
    {
        #region Public Fields

        public IPEndPoint endPoint;
        public byte map;

        #endregion Public Fields
    }
}