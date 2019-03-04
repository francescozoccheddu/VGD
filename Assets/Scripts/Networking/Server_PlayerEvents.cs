using LiteNetLib.Utils;
using Wheeled.Core;
using Wheeled.Gameplay;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed partial class Server : IEventListener
    {

        private sealed class PlayerEventListener : IPlayerEventListener
        {

            private readonly Server m_server;
            private readonly PlayerEntry m_player;

            public PlayerEventListener(Server _server, PlayerEntry _player)
            {
                m_server = _server;
                m_player = _player;
            }

            public void Corrected(int _node, PlayerBehaviour.SimulationState _simulation)
            {
            }

            public void Moved(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(_node);
                writer.Put(_input);
                writer.Put(_calculatedSimulation);
                m_server.SendToAll(writer, LiteNetLib.DeliveryMethod.Sequenced);
            }

        }

    }

}
