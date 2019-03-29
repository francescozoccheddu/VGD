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
                case Message.RoomSync:
                {
                    _reader.ReadRoomSyncMessage(out double time, out PlayerStats stats, out byte health);
                    // Time
                    m_targetTime = time + m_server.Ping / 2.0;
                    if (!m_isRunning)
                    {
                        m_isRunning = true;
                        m_time = m_targetTime;
                    }
                    // Controllers
                    SyncLocalPlayer(time, stats, health);
                    if (!m_localMovementController.IsRunning)
                    {
                        ScheduleLocalPlayerSend();
                        m_localMovementController.StartAt(m_time);
                    }
                }
                break;
                case Message.SimulationOrder:
                {
                    _reader.ReadSimulationCorrectionMessage(out int step, out SimulationStepInfo _simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_localInputHistory.Put(step, _simulation.input);
                    SimulationStep correctedSimulation = m_localInputHistory.SimulateFrom(step, _simulation.simulation);
                    m_localMovementController.Teleport(new Snapshot { sight = m_localMovementController.RawSnapshot.sight, simulation = correctedSimulation }, false);
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
                case Message.SpawnOrder:
                {
                    _reader.ReadSpawnOrderMessage(out double time, out byte spawnPoint);
                    m_localActionHistory.PutSpawn(time);
                    Debug.Log("Spawned");
                }
                break;
                case Message.SpawnReplication:
                {
                    _reader.ReadSpawnReplicationMessage(out double time, out byte id, out byte spawnPoint);
                    GetOrCreatePlayer(id).Spawn(time);
                }
                break;
                case Message.PlayerSync:
                {
                    _reader.ReadPlayerSyncMessage(out double time, out IEnumerable<PlayerSyncInfo> infos);
                    foreach (PlayerSyncInfo info in infos)
                    {
                        GetOrCreatePlayer(info.id).Sync(time, info.info, info.kills, info.deaths, info.deaths);
                    }
                }
                break;
            }
        }

    }

}
