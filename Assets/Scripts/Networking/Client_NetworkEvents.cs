using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed partial class Client : IEventListener
    {

        private readonly NetworkInstance m_network;
        private readonly int m_localPlayerId;
        private readonly Dictionary<byte, Player> m_players;
        private readonly Peer m_server;

        public delegate void DisconnectedEventHandler();

        public event DisconnectedEventHandler OnDisconnected;

        private Player GetOrCreatePlayer(byte _id)
        {
            if (m_players.TryGetValue(_id, out Player _player))
            {
                return _player;
            }
            else
            {
                Player player = new Player();
                player.Setup(null, false, false);
                m_players.Add(_id, player);
                return player;
            }
        }

        public Client(NetworkInstance _network, Peer _server, byte _localPlayerId)
        {
            m_network = _network;
            m_players = new Dictionary<byte, Player>();
            Player localPlayer = new Player();
            localPlayer.Setup(new LocalPlayerEventListener(this), true, false);
            m_players.Add(_localPlayerId, localPlayer);
            m_localPlayerId = _localPlayerId;
            m_server = _server;
        }

        public void ConnectedTo(Peer _peer)
        {
            if (_peer != m_server)
            {
                _peer.Disconnect();
                Debug.Log("Connected");
            }
        }

        public void DisconnectedFrom(Peer _peer)
        {
            if (m_server == _peer)
            {
                OnDisconnected?.Invoke();
            }
        }

        public void ReceivedFrom(Peer _peer, NetPacketReader _reader)
        {
            if (_peer == m_server)
            {
                Message message = _reader.GetEnum<Message>();
                switch (message)
                {
                    case Message.Move:
                    {
                        byte id = _reader.GetByte();
                        int node = _reader.GetInt();
                        PlayerBehaviour.InputState inputState = _reader.GetInputState();
                        PlayerBehaviour.SimulationState simulationState = _reader.GetSimulationState();
                        Player player = GetOrCreatePlayer(id);
                        if (id != m_localPlayerId)
                        {
                            player.Move(node, inputState, simulationState);
                        }
                        else
                        {
                            // TODO Do reconciliation
                        }
                    }
                    break;
                    case Message.UpdatePresentationLatency:
                    break;
                    default:
                    break;
                }
            }
        }

        public bool ShouldAcceptConnectionRequest(Peer _peer, NetDataReader _reader)
        {
            return false;
        }

        public bool ShouldReplyToDiscoveryRequest()
        {
            return false;
        }


    }

}
