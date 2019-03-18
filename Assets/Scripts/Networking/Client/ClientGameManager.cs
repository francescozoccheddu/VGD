using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Debugging;
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

        private double m_time;
        private double m_targetTime;
        private bool m_isRunning;
        private const double c_timeSmoothQuickness = 0.1;

        private NetPlayer GetOrCreatePlayer(int _id)
        {
            if (m_netPlayers.TryGetValue(_id, out NetPlayer netPlayer))
            {
                return netPlayer;
            }
            else
            {
                NetPlayer newNetPlayer = new NetPlayer(this, _id);
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
            m_inputHistory = new InputHistory();
            int maxInputStepCount = (1.0 / c_controllerSendFrequency).CeilingSimulationSteps() + 1;
            m_movementController = new MovementController
            {
                target = this
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
                    _reader.ReadRoomUpdateMessage(out double time);
                    Debug.LogFormat("RoomUpdate at {0} (oldTime={1}, diff={2})", time, m_time, time - m_time);
                    m_targetTime = time + m_server.Ping / 2.0;
                    if (!m_isRunning)
                    {
                        m_isRunning = true;
                        m_time = m_targetTime;
                    }
                    if (!m_movementController.IsRunning)
                    {
                        ScheduleLocalPlayerSend();
                        m_movementController.StartAt(m_time);
                    }
                }
                break;
                case Message.SimulationCorrection:
                {
                    _reader.ReadSimulationCorrectionMessage(out int step, out SimulationStepInfo _simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_inputHistory.Put(step, _simulation.input);
                    SimulationStep correctedSimulation = m_inputHistory.SimulateFrom(step, _simulation.simulation);
                    m_movementController.Teleport(new Snapshot { sight = m_movementController.RawSnapshot.sight, simulation = correctedSimulation }, false);
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
                    GetOrCreatePlayer(id).Move(step, new ArraySegment<InputStep>(m_inputBuffer, 0, inputStepCount), snapshot);
                }
                break;
            }
        }

        void Client.IGameManager.Stopped()
        {
            m_isRunning = false;
            m_updatable.IsRunning = false;
        }

        #endregion

        void Updatable.ITarget.Update()
        {
            if (m_isRunning)
            {
                m_time += Time.deltaTime;
                m_targetTime += Time.deltaTime;
                m_time = m_time * (1.0 - c_timeSmoothQuickness) + m_targetTime * (c_timeSmoothQuickness);
                Printer.Print("Offset", m_targetTime - m_time);
            }
            UpdateLocalPlayer();
            foreach (NetPlayer netPlayer in m_netPlayers.Values)
            {
                netPlayer.Update();
            }
        }

    }

}
