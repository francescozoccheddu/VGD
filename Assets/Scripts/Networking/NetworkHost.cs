namespace Wheeled.Networking
{
    internal interface PlayerEventHandler
    {

    }

    internal interface INetworkHost
    {

        PlayerEventHandler PlayerEvents { get; }
        bool IsRunning { get; }
        void Stop();
        void Update();

    }

}
