using System;
using System.Collections.Generic;
using UnityEngine;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        void Client.IGameManager.Received(Deserializer _reader)
        {
            // TODO Catch exception
            switch (_reader.ReadMessageType())
            {
                #region Room messages
                case Message.TimeSync:
                {
                    _reader.ReadTimeSync(out double time);
                    // Time
                    m_targetTime = time + m_server.Ping / 2.0;
                    if (!m_isRunning)
                    {
                        m_isRunning = true;
                        m_time = m_targetTime;
                    }
                    // Controllers
                    if (!m_localMovementController.IsRunning)
                    {
                        ScheduleLocalPlayerSend();
                        m_localMovementController.StartAt(m_time);
                    }
                }
                break;
                case Message.PlayerIntroductionSync:
                {
                    _reader.ReadPlayerIntroduction(out byte id, out PlayerInfo info);
                    GetOrCreatePlayer(id).Introduce(info);
                }
                break;
                case Message.RecapSync:
                {
                    _reader.ReadRecapSync(out double time, out IEnumerable<PlayerRecapInfo> infos);
                    Dictionary<int, NetPlayer> oldPlayers = new Dictionary<int, NetPlayer>(m_netPlayers);
                    foreach (PlayerRecapInfo info in infos)
                    {
                        if (info.id == m_localPlayerId)
                        {
                            SyncLocalPlayer(time, info.kills, info.deaths, info.health);
                        }
                        else
                        {
                            GetOrCreatePlayer(info.id).Sync(time, info.kills, info.deaths, info.health, info.ping);
                        }
                        oldPlayers.Remove(info.id);
                    }
                    foreach (NetPlayer oldPlayer in oldPlayers.Values)
                    {
                        oldPlayer.Quit(time);
                    }
                }
                break;
                #endregion

                #region Movement messages
                case Message.SimulationOrder:
                {
                    _reader.ReadSimulationCorrection(out int step, out SimulationStepInfo _simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_localInputHistory.Put(step, _simulation.input);
                    SimulationStep correctedSimulation = m_localInputHistory.SimulateFrom(step, _simulation.simulation);
                    m_localMovementController.Teleport(new Snapshot { sight = m_localMovementController.RawSnapshot.sight, simulation = correctedSimulation }, false);
                }
                break;
                case Message.MovementReplication:
                {
                    _reader.ReadMovementReplication(out byte id, out int step, out Snapshot snapshot);
                    GetOrCreatePlayer(id).Move(step, snapshot);
                }
                break;
                case Message.MovementAndInputReplication:
                {
                    _reader.ReadMovementAndInputReplication(out byte id, out int step, out int inputStepCount, m_inputBuffer, out Snapshot snapshot);
                    GetOrCreatePlayer(id).Move(step, new ArraySegment<InputStep>(m_inputBuffer, 0, inputStepCount), snapshot);
                }
                break;
                #endregion

                #region Action messages
                case Message.SpawnOrder:
                {
                    _reader.ReadSpawnOrder(out double time, out byte spawnPoint);
                    m_localActionHistory.PutSpawn(time);
                    Debug.Log("Spawned");
                }
                break;
                case Message.SpawnReplication:
                {
                    _reader.ReadSpawnReplication(out double time, out byte id, out byte spawnPoint);
                    GetOrCreatePlayer(id).Spawn(time);
                }
                break;
                #endregion
            }
        }

    }

}
