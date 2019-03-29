using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Debugging;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
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
            m_localInputHistory = new InputHistory();
            int maxInputStepCount = (1.0 / c_controllerSendFrequency).CeilingSimulationSteps() + 1;
            m_localMovementController = new MovementController
            {
                target = this
            };
            m_localPlayerView = new PlayerView();
            ScheduleLocalPlayerSend();
            m_inputBuffer = new InputStep[Math.Max(maxInputStepCount, c_maxReplicationInputStepCount)];
            m_localActionHistory = new ActionHistory();
            m_localStatsHistory = new LinkedListHistory<double, PlayerStats>();
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
