using System.Net;

namespace Wheeled.Networking.Client
{
    internal sealed partial class Client : Client.IServer, NetworkManager.IEventListener, IGameHost
    {
        #region Public Interfaces

        public interface IServer
        {
            #region Public Properties

            double Ping { get; }

            #endregion Public Properties

            #region Public Methods

            void Send(NetworkManager.SendMethod _method);

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Public Properties

        double IServer.Ping => m_server.Ping;

        #endregion Public Properties

        #region Public Methods

        void IServer.Send(NetworkManager.SendMethod _method)
        {
            m_server.Send(_method);
        }

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
            _reader.ReadDiscoveryInfo(out byte arena);
            OnRoomDiscovered?.Invoke(new GameRoomInfo
            {
                endPoint = _endPoint,
                map = arena
            });
        }

        NetworkManager.DiscoveryRequestAction NetworkManager.IEventListener.DiscoveryRequested(Deserializer _reader)
        {
            return NetworkManager.DiscoveryRequestAction.Ignore;
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, double _latency)
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
                        if (_reader.ReadMessageType() == EMessage.PlayerWelcomeSync)
                        {
                            _reader.ReadPlayerWelcomeSync(out byte id, out byte map);
                            m_localPlayerId = id;
                            IsConnected = true;
                            GameRoomInfo roomInfo = new GameRoomInfo
                            {
                                endPoint = _peer.EndPoint,
                                map = map
                            };
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

        #endregion Public Methods
    }
}