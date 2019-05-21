using System.Collections.Generic;
using Wheeled.Gameplay.Player;
using Wheeled.UI.HUD;
using Wheeled.Networking;

namespace Wheeled.Core
{
    public interface IGameManager
    {
        double Time { get; }

        GameRoomInfo Room { get; }

        IReadOnlyPlayer GetPlayerById(int _id);

        IEnumerable<IReadOnlyPlayer> Players { get; }

        IReadOnlyPlayer LocalPlayer { get; }

    }

    public static class GameManager
    {

        public static IGameManager Current { get; private set; }

        public static void SetCurrentGameManager(IGameManager _current)
        {
            Current = _current;
        }

    }

}