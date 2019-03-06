using LiteNetLib.Utils;
using Wheeled.Core;
using Wheeled.Gameplay;

namespace Wheeled.Networking
{

    internal sealed partial class Client
    {

        private sealed class LocalPlayerEventListener : IPlayerEventListener
        {

            private readonly Client m_server;

            public LocalPlayerEventListener(Client _client)
            {
                m_server = _client;
            }

            public void Corrected(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _simulation)
            {
            }

            public void Moved(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Message.Move);
                writer.Put(_node);
                writer.Put(_input);
                writer.Put(_calculatedSimulation);
                m_server.m_server.Send(writer, LiteNetLib.DeliveryMethod.Unreliable);
            }

        }

    }

}
