using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using Wheeled.Core;

namespace Wheeled.Networking
{
    internal abstract class NetworkHost : NetworkManager.EventListener
    {

        protected PlayerBehaviour m_LocalPlayerBehaviour => m_localPlayer.Behaviour;

        protected void NetPlayerDo(NetPeer _netPeer, Action<PlayerBehaviour> _action)
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

        protected void DestroyNetPlayer(NetPeer _netPeer)
        {
            if (m_netPlayers.TryGetValue(_netPeer, out Player p))
            {
                p.Destroy();
            }
            m_netPlayers[_netPeer] = null;
        }

        private readonly Player m_localPlayer;
        private readonly Dictionary<NetPeer, Player> m_netPlayers;
        private bool m_IsGameSceneLoaded;

        protected NetworkHost()
        {
            m_localPlayer = new Player();
            m_netPlayers = new Dictionary<NetPeer, Player>();
            m_IsGameSceneLoaded = false;
        }

        public abstract void ConnectedTo(NetworkManager.IPeer _peer);
        public abstract void DisconnectedFrom(NetworkManager.IPeer _peer);
        public abstract void ReceivedFrom(NetworkManager.IPeer _peer, NetPacketReader _reader);
        public abstract bool ShouldAcceptConnectionRequest(NetDataReader _reader);
        public abstract bool ShouldReplyToDiscoveryRequest();

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

    }
}
