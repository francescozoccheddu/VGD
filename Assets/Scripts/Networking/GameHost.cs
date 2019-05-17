using System.Collections.Generic;
using Wheeled.Gameplay.Player;
using Wheeled.HUD;

namespace Wheeled.Networking
{
    public delegate void GameHostStopped(EGameHostStopCause _cause);

    public enum EGameHostStopCause
    {
        Programmatically, NetworkError, Disconnected, UnableToConnect
    }

    public interface IGameHost 
    {
        event GameHostStopped OnStopped;

        bool IsStarted { get; }

        GameRoomInfo? RoomInfo { get; }

        void GameReady();

        void Stop();
    }
}