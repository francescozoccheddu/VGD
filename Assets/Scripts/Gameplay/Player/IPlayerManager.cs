using Wheeled.HUD;

namespace Wheeled.Gameplay.Player
{
    public interface IPlayerManager
    {
        EventBoardDispatcher MatchBoard { get; }
        double Time { get; }
    }
}