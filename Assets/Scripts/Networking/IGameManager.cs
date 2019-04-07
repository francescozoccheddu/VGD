using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking
{
    internal interface IGameManager
    {
        OffenseStage ShootStage { get; }
        double Time { get; }
    }
}