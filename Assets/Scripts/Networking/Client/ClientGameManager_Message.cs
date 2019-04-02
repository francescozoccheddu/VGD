using System.Collections.Generic;

using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
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
                    m_localPlayer.EnsureStarted();
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
                    Dictionary<byte, Player> oldPlayers = new Dictionary<byte, Player>(m_players);
                    foreach (PlayerRecapInfo info in infos)
                    {
                        GetOrCreatePlayer(info.id).Sync(time, info.kills, info.deaths, info.health);
                        oldPlayers.Remove(info.id);
                    }
                    foreach (NetPlayer oldPlayer in oldPlayers.Values)
                    {
                        oldPlayer.Quit(time);
                    }
                }
                break;

                #endregion Room messages

                #region Movement messages

                case Message.SimulationOrder:
                {
                    _reader.ReadSimulationCorrection(out int step, out SimulationStepInfo simulation);
                    Debug.LogFormat("Reconciliation {0}", step);
                    m_localPlayer.Correct(step, simulation);
                }
                break;

                case Message.MovementReplication:
                {
                    _reader.ReadMovementReplication(out byte id, out int step, out IEnumerable<InputStep> inputSteps, out Snapshot snapshot);
                    NetPlayer player = GetOrCreatePlayer(id) as NetPlayer;
                    player?.Move(step, inputSteps, snapshot);
                }
                break;

                #endregion Movement messages

                #region Action messages

                case Message.SpawnOrderOrReplication:
                {
                    _reader.ReadSpawnOrderOrReplication(out double time, out byte id, out SpawnInfo spawnInfo);
                    GetOrCreatePlayer(id).Spawn(time);
                }
                break;

                case Message.DeathOrderOrReplication:
                {
                    _reader.ReadDeathOrderOrReplication(out double time, out DeathInfo deathInfo);
                    GetOrCreatePlayer(deathInfo.deadId).Die(time, deathInfo.explosion);
                }
                break;

                #endregion Action messages
            }
        }
    }
}