using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking
{
    internal interface IGameManager
    {
        ShootStage ShootStage { get; }
        double Time { get; }
    }
}