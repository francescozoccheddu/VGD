using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Wheeled.Core;
using static Wheeled.Networking.NetworkManager;

namespace Wheeled.Networking
{
    using NetPlayersDictKey = NetPeer;

    internal abstract class NetworkHost : EventListener, PlayerEventListener
    {

        protected readonly NetworkInstance m_netInstance;

        protected PlayerBehaviour m_LocalPlayerBehaviour => m_localPlayer.Behaviour;

        private NetPlayersDictKey GetPlayerDictKey(Player _player)
        {
            return m_netPlayers.FirstOrDefault(p => p.Value == _player).Key;
        }

        protected void NetPlayerDo(NetPlayersDictKey _netPeer, Action<PlayerBehaviour> _action)
        {
            if (!m_netPlayers.TryGetValue(_netPeer, out Player player))
            {
                player = new Player();
                if (m_IsGameSceneLoaded)
                {
                    player.Instantiate();
                }
                m_netPlayers.Add(_netPeer, player);
            }
            if (player != null)
            {
                _action(player.Behaviour);
            }
        }

        protected void DestroyNetPlayer(NetPlayersDictKey _netPeer)
        {
            if (m_netPlayers.TryGetValue(_netPeer, out Player p))
            {
                p.Destroy();
            }
            m_netPlayers[_netPeer] = null;
        }

        private readonly Player m_localPlayer;
        private readonly Dictionary<NetPlayersDictKey, Player> m_netPlayers;
        private bool m_IsGameSceneLoaded;

        protected NetworkHost(NetworkInstance _netInstance)
        {
            m_netInstance = _netInstance;
            m_localPlayer = new Player();
            m_netPlayers = new Dictionary<NetPlayersDictKey, Player>();
            m_IsGameSceneLoaded = false;
        }

        public abstract void ConnectedTo(IPeer _peer);
        public abstract void DisconnectedFrom(IPeer _peer);
        public abstract void ReceivedFrom(IPeer _peer, NetPacketReader _reader);
        public abstract bool ShouldAcceptConnectionRequest(NetDataReader _reader);
        public abstract bool ShouldReplyToDiscoveryRequest();

        public abstract void Moved(NetPlayersDictKey _key);

        public void GameSceneLoaded()
        {
            if (!m_IsGameSceneLoaded)
            {
                m_localPlayer.Instantiate();
                foreach (Player p in m_netPlayers.Values)
                {
                    p.Instantiate();
                }
                m_IsGameSceneLoaded = true;
            }
        }

        public void Moved(Player _player)
        {
            Moved(GetPlayerDictKey(_player));
        }
    }
}
