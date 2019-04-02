using System.Net;

namespace Wheeled.Networking.Client
{
    internal sealed partial class Client : Client.IServer, NetworkManager.IEventListener, IGameHost
    {
        public interface IServer
        {
            float Ping { get; }

            void Send(NetworkManager.SendMethod _method);
        }

        #region Client.IServer

        float IServer.Ping => m_server.Ping;

        void IServer.Send(NetworkManager.SendMethod _method)
        {
            m_server.Send(_method);
        }

        #endregion Client.IServer

        #region NetworkManager.IEventListener

        void NetworkManager.IEventListener.ConnectedTo(NetworkManager.Peer _peer)
        {
            if (_peer != m_server)
            {
                _peer.Disconnect();
            }
        }

        void NetworkManager.IEventListener.DisconnectedFrom(NetworkManager.Peer _peer)
        {
            if (_peer == m_server)
            {
                bool wasConnected = IsConnected;
                Cleanup();
                NotifyStopped(wasConnected ? GameHostStopCause.Disconnected : GameHostStopCause.UnableToConnect);
            }
        }

        void NetworkManager.IEventListener.Discovered(IPEndPoint _endPoint, Deserializer _reader)
        {
            // TODO Parse info
            OnRoomDiscovered?.Invoke(new GameRoomInfo(_endPoint, "", 0));
        }

        NetworkManager.DiscoveryRequestAction NetworkManager.IEventListener.DiscoveryRequested(Deserializer _reader)
        {
            return NetworkManager.DiscoveryRequestAction.Ignore;
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, float _latency)
        {
            if (_peer == m_server)
            {
                m_game?.LatencyUpdated(_latency);
            }
            else
            {
                _peer.Disconnect();
            }
        }

        void NetworkManager.IEventListener.ReceivedFrom(NetworkManager.Peer _peer, Deserializer _reader)
        {
            if (_peer == m_server)
            {
                if (!IsConnected)
                {
                    try
                    {
                        if (_reader.ReadMessageType() == Message.PlayerWelcomeSync)
                        {
                            _reader.ReadPlayerWelcomeSync(out byte id);
                            m_localPlayerId = id;
                            IsConnected = true;
                            GameRoomInfo roomInfo = new GameRoomInfo(null, "", 0);
                            RoomInfo = roomInfo;
                            OnConnected?.Invoke(roomInfo);
                        }
                    }
                    catch (Deserializer.DeserializationException)
                    {
                        Cleanup();
                        NotifyStopped(GameHostStopCause.UnableToConnect);
                    }
                }
                else
                {
                    m_game?.Received(_reader);
                }
            }
            else
            {
                _peer.Disconnect();
            }
        }

        bool NetworkManager.IEventListener.ShouldAcceptConnectionRequest(NetworkManager.Peer _peer, Deserializer _reader)
        {
            return false;
        }

        void NetworkManager.IEventListener.Stopped(NetworkManager.StopCause _cause)
        {
            NotifyStopped(GameHostStopCause.NetworkError);
        }

        #endregion NetworkManager.IEventListener
    }
}