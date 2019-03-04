using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    internal sealed partial class Client
    {


        private sealed class PlayerEventListener : IPlayerEventListener
        {

            private readonly Client m_server;
            private readonly Player m_player;

            public PlayerEventListener(Client _client, Player _player)
            {
                m_server = _client;
                m_player = _player;
            }

            public void Corrected(int _node, PlayerBehaviour.SimulationState _simulation)
            {
            }

            public void Moved(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation)
            {
            }

        }

    }

}
