using System.Net;

namespace Wheeled.Networking.Client
{
    public sealed partial class Client : Client.IServer, NetworkManager.IEventListener, IGameHost
    {
        public interface IServer
        {
            double Ping { get; }

            void Send(NetworkManager.ESendMethod _method);
        }

        double IServer.Ping => m_server.Ping;

        void IServer.Send(NetworkManager.ESendMethod _method)
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
                NotifyStopped(wasConnected ? EGameHostStopCause.Disconnected : EGameHostStopCause.UnableToConnect);
            }
        }

        void NetworkManager.IEventListener.Discovered(IPEndPoint _endPoint, Deserializer _reader)
        {
            _reader.ReadDiscoveryInfo(out int arena);
            OnRoomDiscovered?.Invoke(new GameRoomInfo
            {
                endPoint = _endPoint,
                arena = arena
            });
        }

        NetworkManager.EDiscoveryRequestAction NetworkManager.IEventListener.DiscoveryRequested(Deserializer _reader)
        {
            return NetworkManager.EDiscoveryRequestAction.Ignore;
        }

        void NetworkManager.IEventListener.LatencyUpdated(NetworkManager.Peer _peer, double _latency)
        {
            if (_peer == m_server)
            {
                ((IGameManager)m_game)?.LatencyUpdated(_latency);
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
                            _reader.ReadPlayerWelcomeSync(out int id, out int map);
                            m_localPlayerId = id;
                            IsConnected = true;
                            GameRoomInfo roomInfo = new GameRoomInfo
                            {
                                endPoint = _peer.EndPoint,
                                arena = map
                            };
                            RoomInfo = roomInfo;
                            OnConnected?.Invoke(roomInfo);
                        }
                    }
                    catch (Deserializer.DeserializationException)
                    {
                        Cleanup();
                        NotifyStopped(EGameHostStopCause.UnableToConnect);
                    }
                }
                else
                {
                    ((IGameManager) m_game)?.Received(_reader);
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

        void NetworkManager.IEventListener.Stopped(NetworkManager.EStopCause _cause)
        {
            NotifyStopped(EGameHostStopCause.NetworkError);
        }
    }
}