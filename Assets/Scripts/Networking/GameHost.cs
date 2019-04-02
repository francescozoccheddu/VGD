namespace Wheeled.Networking
{
    internal delegate void GameHostStopped(GameHostStopCause _cause);

    internal enum GameHostStopCause
    {
        Programmatically, NetworkError, Disconnected, UnableToConnect
    }

    internal interface IGameHost
    {
        event GameHostStopped OnStopped;

        bool IsStarted { get; }
        GameRoomInfo? RoomInfo { get; }

        void GameReady();

        void Stop();
    }
}