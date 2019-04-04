using System.Collections.Generic;

using UnityEngine;

using Wheeled.Core.Utils;
using Wheeled.Debugging;
using Wheeled.Gameplay.Stage;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager : Updatable.ITarget, Client.IGameManager
    {
        private const int c_maxReplicationInputStepCount = 20;
        private const double c_timeSmoothQuickness = 0.1;
        private readonly LocalPlayer m_localPlayer;
        private readonly Dictionary<byte, Player> m_players;
        private readonly Client.IServer m_server;
        private readonly ShootStage m_shootStage;
        private readonly Updatable m_updatable;
        private bool m_isRunning;
        private double m_targetTime;
        private double m_time;

        public ClientGameManager(Client.IServer _server, byte _id)
        {
            Debug.Log("ClientGameManager started");
            m_shootStage = new ShootStage();
            m_updatable = new Updatable(this, false)
            {
                IsRunning = true
            };
            m_server = _server;
            m_localPlayer = new LocalPlayer(this, _id)
            {
                ControllerOffset = 0.5,
                HistoryDuration = 5.0,
                MaxMovementInputStepsNotifyCount = 5,
                MaxMovementNotifyFrequency = 5
            };
            m_players = new Dictionary<byte, Player>
            {
                { _id, m_localPlayer }
            };
            // Ready notify
            Serializer.WriteReady();
            m_server.Send(NetworkManager.SendMethod.ReliableUnordered);
        }

        void Updatable.ITarget.Update()
        {
            if (m_isRunning)
            {
                m_time += Time.deltaTime;
                m_targetTime += Time.deltaTime;
                m_time = m_time * (1.0 - c_timeSmoothQuickness) + m_targetTime * (c_timeSmoothQuickness);
                Printer.Print("Offset", m_targetTime - m_time);
            }
            foreach (Player p in m_players.Values)
            {
                p.Update();
            }
            m_shootStage.Update(m_time);
        }

        private Player GetOrCreatePlayer(byte _id)
        {
            if (m_players.TryGetValue(_id, out Player player))
            {
                return player;
            }
            else
            {
                NetPlayer newNetPlayer = new NetPlayer(this, _id)
                {
                    HistoryDuration = 5.0,
                    HistoryOffset = 0.5,
                };
                m_players.Add(_id, newNetPlayer);
                return newNetPlayer;
            }
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

        #endregion Client.IGameManager
    }
}