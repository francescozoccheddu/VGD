using System.Collections.Generic;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;

namespace Wheeled.Networking
{
    public delegate void GameHostStopped(EGameHostStopCause _cause);


    public static class GameHostHelper
    {

        public static string GetHumanReadableMessage(this EGameHostStopCause _stopCause)
        {
            switch (_stopCause)
            {
                case EGameHostStopCause.Programmatically:
                return "Interrupted by user";
                case EGameHostStopCause.NetworkError:
                return "Network error";
                case EGameHostStopCause.Disconnected:
                return "Disconnected";
                case EGameHostStopCause.UnableToConnect:
                return "Connection failed";
                default:
                return "Unknown stop cause";
            }
        }

    }

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