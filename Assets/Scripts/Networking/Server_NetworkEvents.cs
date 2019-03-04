using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using Wheeled.Core;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed partial class Server : IEventListener
    {

        private const int c_maxPlayerCount = 4;

        private readonly struct PlayerEntry
        {
            public readonly byte id;
            public readonly Player player;

            public PlayerEntry(byte _id, Player _player)
            {
                id = _id;
                player = _player;
            }
        }

        private readonly NetworkInstance m_network;
        private readonly PlayerEntry m_localPlayer;
        private readonly Dictionary<Peer, PlayerEntry> m_netPlayers;
        private byte m_nextPlayerId;

        private PlayerEntry CreateNewPlayer(bool _isInteractive, bool _isAuthoritative)
        {
            PlayerEntry entry = new PlayerEntry(m_nextPlayerId++, new Player());
            PlayerEventListener eventListener = new PlayerEventListener(this, entry);
            entry.player.Setup(eventListener, _isInteractive, _isAuthoritative);
            return entry;
        }

        private void SendToAll(NetDataWriter _dataWriter, DeliveryMethod _method)
        {
            foreach (Peer _peer in m_netPlayers.Keys)
            {
                _peer.Send(_dataWriter, _method);
            }
        }

        public Server(NetworkInstance _network)
        {
            m_network = _network;
            m_localPlayer = CreateNewPlayer(true, true);
            m_netPlayers = new Dictionary<Peer, PlayerEntry>();
        }

        public void ConnectedTo(Peer _peer)
        {
            if (m_netPlayers.TryGetValue(_peer, out PlayerEntry playerEntry))
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Message.Welcome);
                writer.Put(playerEntry.id);
                _peer.Send(writer, DeliveryMethod.ReliableUnordered);
            }
        }

        public void DisconnectedFrom(Peer _peer)
        {
            if (m_netPlayers.TryGetValue(_peer, out PlayerEntry entry))
            {
                entry.player.Destroy();
                m_netPlayers.Remove(_peer);
            }
        }

        public void ReceivedFrom(Peer _peer, NetPacketReader _reader)
        {
            Message message = _reader.GetEnum<Message>();
            switch (message)
            {
                case Message.Move:
                {
                    if (m_netPlayers.TryGetValue(_peer, out PlayerEntry playerEntry))
                    {
                        playerEntry.player.Move(_reader.GetInt(), _reader.GetInputState(), _reader.GetSimulationState());
                    }
                }
                break;
                case Message.UpdatePresentationLatency:
                break;
                case Message.Welcome:
                break;
            }
        }

        public bool ShouldAcceptConnectionRequest(Peer _peer, NetDataReader _reader)
        {
            m_netPlayers.Add(_peer, CreateNewPlayer(false, false));
            return m_netPlayers.Count + 1 < c_maxPlayerCount;
        }

        public bool ShouldReplyToDiscoveryRequest()
        {
            return m_netPlayers.Count + 1 < c_maxPlayerCount;
        }


    }

}
