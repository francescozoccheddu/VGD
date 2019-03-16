using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager
    {

        private const int c_maxReplicationInputStepCount = 20;

        private readonly InputStep[] m_inputBuffer;
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
            // Local player
            int maxInputStepCount = TimeStep.GetStepsInPeriod(1.0f / c_controllerSendFrequency) + 1;
            m_movementController = new MovementController(3.0f)
            {
                InputBufferSize = maxInputStepCount
            };
            m_view = new PlayerView();
            ScheduleLocalPlayerSend();
            m_inputBuffer = new InputStep[Math.Max(maxInputStepCount, c_maxReplicationInputStepCount)];
            // Net players
            m_netPlayers = new Dictionary<int, NetPlayer>();
            // Ready notify
            Serializer.WriteReadyMessage();
            m_server.Send(NetworkManager.SendMethod.ReliableUnordered);
        }

        #region Client.IGameManager

        void Client.IGameManager.LatencyUpdated(float _latency)
        {
        }

        void Client.IGameManager.Received(Deserializer _reader)
        {
            // TODO Catch exception
            switch (_reader.ReadMessageType())
            {
                case Message.RoomUpdate:
                {
                    _reader.ReadRoomUpdateMessage(out TimeStep time);
                    Debug.LogFormat("RoomUpdate at {0} (oldTime={1}, diff={2})", time, RoomTime.Now, time - RoomTime.Now);
                    RoomTime.Manager.Set(time + m_server.Ping / 2.0f, RoomTime.IsRunning);
                    RoomTime.Manager.Start();
                    if (!m_movementController.IsRunning)
                    {
                        ScheduleLocalPlayerSend();
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
                case Message.MovementAndInputReplication:
                {
                    _reader.ReadMovementAndInputReplicationMessage(out byte id, out int step, out int inputStepCount, m_inputBuffer, out Snapshot snapshot);
                    if (inputStepCount > m_inputBuffer.Length)
                    {
                        step += inputStepCount - m_inputBuffer.Length;
                        inputStepCount = m_inputBuffer.Length;
                    }
                    GetOrCreatePlayer(id).Move(step, new ArraySegment<InputStep>(m_inputBuffer, 0, inputStepCount), snapshot);
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

        void Updatable.ITarget.Update()
        {
            RoomTime.Manager.Update();
            UpdateLocalPlayer();
            foreach (NetPlayer netPlayer in m_netPlayers.Values)
            {
                netPlayer.Update();
            }
        }

    }

}
