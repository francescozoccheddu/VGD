using System.Collections.Generic;

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
                        Player player = GetOrCreatePlayer(info.id);
                        player.PutDeaths(time, info.deaths);
                        player.PutHealth(time, info.health);
                        player.PutKills(time, info.kills);
                        oldPlayers.Remove(info.id);
                    }
                    foreach (NetPlayer oldPlayer in oldPlayers.Values)
                    {
                        oldPlayer.PutQuit(time);
                    }
                }
                break;

                case Message.QuitReplication:
                {
                    _reader.ReadQuitReplication(out double time, out byte id);
                    if (m_players.TryGetValue(id, out Player player))
                    {
                        player.PutQuit(time);
                    }
                }
                break;

                #endregion Room messages

                #region Movement messages

                case Message.SimulationOrder:
                {
                    _reader.ReadSimulationOrder(out int step, out SimulationStepInfo simulation);
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
                    GetOrCreatePlayer(id).PutSpawn(time, spawnInfo);
                }
                break;

                case Message.DeathOrderOrReplication:
                {
                    _reader.ReadDeathOrderOrReplication(out double time, out byte id, out DeathInfo deathInfo, out byte deaths, out byte kills);
                    GetOrCreatePlayer(id).PutDeath(time, deathInfo, deaths);
                    GetOrCreatePlayer(id).PutKills(time, kills);
                }
                break;

                case Message.HitConfirmOrder:
                {
                    _reader.ReadHitConfirmOrder(out double time, out HitConfirmInfo info);
                    m_localPlayer.PutHitConfirm(time, info);
                }
                break;

                case Message.DamageOrder:
                {
                    _reader.ReadDamageOrder(out double time, out DamageInfo info, out byte health);
                    m_localPlayer.PutDamage(time, info, health);
                }
                break;

                case Message.ShootReplication:
                {
                    _reader.ReadShotReplication(out double time, out byte id, out ShotInfo info);
                    NetPlayer player = GetOrCreatePlayer(id) as NetPlayer;
                    player?.PutShoot(time, info);
                }
                break;

                #endregion Action messages
            }
        }
    }
}