using Wheeled.Assets.Scripts.Networking;

namespace Wheeled.Networking
{

    internal enum GameHostStopCause
    {
        Programmatically, NetworkError, Disconnected, UnableToConnect
    }

    internal delegate void GameHostStopped(GameHostStopCause _cause);

    internal interface IGameHost
    {

        GameRoomInfo? RoomInfo { get; }
        bool IsStarted { get; }

        void GameReady();

        void Stop();


        event GameHostStopped OnStopped;

    }

}
