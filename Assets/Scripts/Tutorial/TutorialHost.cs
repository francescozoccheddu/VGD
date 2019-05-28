using Wheeled.Networking;

namespace Wheeled.Tutorial
{
    public sealed class TutorialHost : IGameHost
    {
        public bool IsStarted { get; private set; }

        GameRoomInfo? IGameHost.RoomInfo => null;

        public event GameHostStopped OnStopped;

        void IGameHost.GameReady()
        {
            if (!IsStarted)
            {
                IsStarted = true;
                new TutorialGameManager();
            }
        }

        void IGameHost.Stop()
        {
            if (IsStarted)
            {
                IsStarted = false;
                OnStopped?.Invoke(EGameHostStopCause.Programmatically);
            }
        }

    }
}
