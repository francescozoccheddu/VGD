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
        private readonly byte m_localPlayerId;
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
                    case Message.Moved:
                    {
                        byte id = _reader.GetByte();
                        int node = _reader.GetInt();
                        PlayerBehaviour.InputState inputState = _reader.GetInputState();
                        PlayerBehaviour.SimulationState simulationState = _reader.GetSimulationState();
                        Player player = GetOrCreatePlayer(id);
                        if (id != m_localPlayerId)
                        {
                            player.Do(_p => _p.Moved(node, inputState, simulationState));
                        }
                    }
                    break;
                    case Message.Spawned:
                    {
                        byte id = _reader.GetByte();
                        PlayerBehaviour.Time time = _reader.GetTime();
                        byte spawnPoint = _reader.GetByte();
                        Player player = GetOrCreatePlayer(id);
                        player.Do(_p => _p.Spawned(time, spawnPoint));
                    }
                    break;
                    case Message.Died:
                    {
                        byte id = _reader.GetByte();
                        PlayerBehaviour.Time time = _reader.GetTime();
                        Vector3 hitDirection = _reader.GetVector3();
                        Vector3 hitPoint = _reader.GetVector3();
                        bool exploded = _reader.GetBool();
                        Player player = GetOrCreatePlayer(id);
                        player.Do(_p => _p.Died(time, hitDirection, hitPoint, exploded));
                    }
                    break;
                    case Message.Corrected:
                    {
                        int node = _reader.GetInt();
                        PlayerBehaviour.InputState inputState = _reader.GetInputState();
                        PlayerBehaviour.SimulationState simulationState = _reader.GetSimulationState();
                        m_players[m_localPlayerId].Do(_p => _p.Corrected(node, inputState, simulationState));
                    }
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

        public void LatencyUpdated(Peer _peer, int _latency)
        {
            foreach (KeyValuePair<byte, Player> entry in m_players)
            {
                if (entry.Key != m_localPlayerId)
                {
                    entry.Value.DoPOA(_latency / 1000.0f);
                }
            }
        }
    }

}
