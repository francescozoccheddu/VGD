using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using Wheeled.Core;
using Wheeled.Gameplay;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{

    internal sealed partial class Server : IEventListener
    {

        private const int c_maxPlayerCount = 4;

        private struct PlayerEntry
        {
            public readonly byte id;
            public readonly Player player;
            public bool spoken;

            public PlayerEntry(byte _id, Player _player)
            {
                id = _id;
                player = _player;
                spoken = false;
            }
        }

        private readonly NetworkInstance m_network;
        private readonly PlayerEntry m_localPlayer;
        private readonly Dictionary<Peer, PlayerEntry> m_netPlayers;
        private byte m_nextPlayerId;

        private bool TryGetPeerByPlayerId(int _id, out Peer _outPeer)
        {
            foreach (KeyValuePair<Peer, PlayerEntry> entry in m_netPlayers)
            {
                if (entry.Value.id == _id)
                {
                    _outPeer = entry.Key;
                    return true;
                }
            }
            _outPeer = default;
            return false;
        }

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

        private void SendToAllBut(NetDataWriter _dataWriter, DeliveryMethod _method, Peer _but)
        {
            foreach (Peer _peer in m_netPlayers.Keys)
            {
                if (_peer != _but)
                {
                    _peer.Send(_dataWriter, _method);
                }
            }
        }

        public Server(NetworkInstance _network)
        {
            m_network = _network;
            m_localPlayer = CreateNewPlayer(true, true);
            m_localPlayer.spoken = true;
            m_localPlayer.player.CanSpawn();
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

        private void UpdateSpoken(Peer _peer, PlayerEntry _playerEntry)
        {
            if (!_playerEntry.spoken)
            {
                _playerEntry.spoken = true;
                _playerEntry.player.CanSpawn();
                NetDataWriter writer = new NetDataWriter();
                foreach (PlayerEntry player in m_netPlayers.Values)
                {
                    if (player.id != _playerEntry.id)
                    {
                        player.player.GetSpawnInfo(out PlayerBehaviour.Time statusTime, out byte? spawnPoint);
                        writer.Reset();
                        writer.Put(Message.Spawned);
                        writer.Put(player.id);
                        writer.Put(statusTime);
                        writer.Put(spawnPoint ?? 255);
                        _peer.Send(writer, DeliveryMethod.ReliableUnordered);
                    }
                }
                m_netPlayers[_peer] = _playerEntry;
            }
        }

        public void ReceivedFrom(Peer _peer, NetPacketReader _reader)
        {
            Message message = _reader.GetEnum<Message>();
            switch (message)
            {
                case Message.Moved:
                {
                    if (m_netPlayers.TryGetValue(_peer, out PlayerEntry playerEntry))
                    {
                        UpdateSpoken(_peer, playerEntry);
                        playerEntry.player.Do(_p => _p.Moved(_reader.GetInt(), _reader.GetInputState(), _reader.GetSimulationState()));
                    }
                }
                break;
            }
        }

        public bool ShouldAcceptConnectionRequest(Peer _peer, NetDataReader _reader)
        {
            m_netPlayers.Add(_peer, CreateNewPlayer(false, true));
            return m_netPlayers.Count + 1 < c_maxPlayerCount;
        }

        public bool ShouldReplyToDiscoveryRequest()
        {
            return m_netPlayers.Count + 1 < c_maxPlayerCount;
        }

        private void DoPOA()
        {
            foreach (KeyValuePair<Peer, PlayerEntry> entry in m_netPlayers)
            {
                entry.Value.player.DoPOA(entry.Key.Ping / 1000.0f);
            }
        }

        public void LatencyUpdated(Peer _peer, int _latency)
        {
            if (m_netPlayers.TryGetValue(_peer, out PlayerEntry playerEntry))
            {
                playerEntry.player.DoPOA(_latency / 1000.0f);
            }
        }
    }

}
