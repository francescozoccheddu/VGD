using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager
    {

        private readonly Updatable m_updatable;
        private readonly Client.IServer m_server;
        private readonly Dictionary<int, NetPlayer> m_netPlayers;

        private NetPlayer GetOrCreatePlayer(int _id)
        {
            if (m_netPlayers.TryGetValue(_id, out NetPlayer netPlayer))
            {
                return netPlayer;
            }
            else
            {
                NetPlayer newNetPlayer = new NetPlayer(_id);
                m_netPlayers.Add(_id, newNetPlayer);
                return newNetPlayer;
            }
        }

        public ClientGameManager(Client.IServer _server)
        {
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_server = _server;
            Debug.Log("ClientGameManager constructed");
            m_movementController = new MovementController(3.0f);
            m_view = new PlayerView();
            m_netPlayers = new Dictionary<int, NetPlayer>();
            Serializer.WriteReadyMessage();
            m_server.Send(DeliveryMethod.ReliableUnordered);
        }

        #region Client.IGameManager

        void Client.IGameManager.LatencyUpdated(float _latency)
        {
        }

        void Client.IGameManager.Received(NetDataReader _reader)
        {
            switch (_reader.ReadMessageType())
            {
                case Message.RoomUpdate:
                {
                    // TODO Check if time is corrupted
                    _reader.ReadRoomUpdateMessage(out TimeStep time);
                    Debug.LogFormat("RoomUpdate at {0} (oldTime={1}, diff={2})", time, RoomTime.Now, time - RoomTime.Now);
                    RoomTime.Manager.Set(time + m_server.Ping / 2.0f, RoomTime.IsRunning);
                    RoomTime.Manager.Start();
                    if (!m_movementController.IsRunning)
                    {
                        m_movementController.StartAt(RoomTime.Now);
                    }
                }
                break;
                case Message.SimulationCorrection:
                {
                    _reader.ReadSimulationCorrectionMessage(out int step, out SimulationStepInfo _simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_movementController.Correct(step, _simulation);
                }
                break;
                case Message.MovementReplication:
                {
                    _reader.ReadMovementReplicationMessage(out byte id, out int step, out Snapshot snapshot);
                    GetOrCreatePlayer(id).Move(step, snapshot);
                }
                break;
            }
        }

        void Client.IGameManager.Stopped()
        {
            RoomTime.Manager.Stop();
            m_updatable.IsRunning = false;
        }

        #endregion

        #region Room Update

        void Updatable.ITarget.Update()
        {
            RoomTime.Manager.Update();
            UpdateLocalPlayer();
        }

        #endregion

    }

}
