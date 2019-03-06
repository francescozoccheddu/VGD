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

            public void Corrected(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _simulation)
            {
                if (m_server.TryGetPeerByPlayerId(m_player.id, out Peer peer))
                {
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(Message.Reconciliate);
                    writer.Put(_node);
                    writer.Put(_input);
                    writer.Put(_simulation);
                    peer.Send(writer, LiteNetLib.DeliveryMethod.Unreliable);
                }
            }

            public void Moved(int _node, PlayerBehaviour.InputState _input, PlayerBehaviour.SimulationState _calculatedSimulation)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Message.Move);
                writer.Put(m_player.id);
                writer.Put(_node);
                writer.Put(_input);
                writer.Put(_calculatedSimulation);
                if (m_player.id == 0)
                {
                    m_server.SendToAll(writer, LiteNetLib.DeliveryMethod.Unreliable);
                }
                else if (m_server.TryGetPeerByPlayerId(m_player.id, out Peer peer))
                {
                    m_server.SendToAllBut(writer, LiteNetLib.DeliveryMethod.Unreliable, peer);
                }
            }

        }

    }

}
