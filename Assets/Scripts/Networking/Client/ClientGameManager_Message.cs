using System.Collections.Generic;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;
using Wheeled.HUD;

namespace Wheeled.Networking.Client
{
    public sealed partial class ClientGameManager
    {
        private const int c_maxStepAdvance = 30;

        void Client.IGameManager.Received(Deserializer _reader)
        {
            // TODO Catch exception
            switch (_reader.ReadMessageType())
            {
                case EMessage.TimeSync:
                {
                    _reader.ReadTimeSync(out double time);
                    // Time
                    m_targetTime = time + m_server.Ping;
                    if (!m_isRunning)
                    {
                        m_isRunning = true;
                        m_time = m_targetTime;
                    }
                }
                break;

                case EMessage.PlayerIntroductionSync:
                {
                    _reader.ReadPlayerIntroduction(out int id, out PlayerInfo info);
                    GetOrCreatePlayer(id).Info = info;
                    UpdateScoreBoard();
                }
                break;

                case EMessage.RecapSync:
                {
                    _reader.ReadRecapSync(out double time, out IEnumerable<PlayerRecapInfo> infos);
                    Dictionary<int, ClientPlayer> oldPlayers = new Dictionary<int, ClientPlayer>(m_players);
                    foreach (PlayerRecapInfo info in infos)
                    {
                        Player player = GetOrCreatePlayer(info.id);
                        player.DeathsValue.Put(time, info.deaths);
                        player.PutHealth(time, info.health);
                        player.KillsValue.Put(time, info.kills);
                        oldPlayers.Remove(info.id);
                    }
                    foreach (ClientPlayer oldPlayer in oldPlayers.Values)
                    {
                        oldPlayer.PutQuit(time);
                    }
                    UpdateScoreBoard();
                }
                break;

                case EMessage.QuitReplication:
                {
                    _reader.ReadQuitReplication(out double time, out int id);
                    if (m_players.TryGetValue(id, out ClientPlayer player))
                    {
                        player.PutQuit(time);
                    }
                    UpdateScoreBoard();
                }
                break;

                case EMessage.SimulationOrder:
                {
                    _reader.ReadSimulationOrder(out int step, out SimulationStepInfo simulation);
                    if (m_isRunning && step > m_time.CeilingSimulationSteps() + c_maxStepAdvance)
                    {
                        break;
                    }
                    m_localPlayer.Correct(step, simulation);
                }
                break;

                case EMessage.MovementReplication:
                {
                    _reader.ReadMovementReplication(out int id, out int step, out IEnumerable<InputStep> inputSteps, out Snapshot snapshot);
                    if (m_isRunning && step > m_time.CeilingSimulationSteps() + c_maxStepAdvance)
                    {
                        break;
                    }
                    NetPlayer player = GetOrCreatePlayer(id) as NetPlayer;
                    player?.SignalReplication();
                    player?.Move(step, inputSteps, snapshot);
                }
                break;

                case EMessage.SpawnOrderOrReplication:
                {
                    _reader.ReadSpawnOrderOrReplication(out double time, out int id, out SpawnInfo spawnInfo);
                    GetOrCreatePlayer(id).PutSpawn(time, spawnInfo);
                }
                break;

                case EMessage.DamageOrderOrReplication:
                {
                    _reader.ReadDamageOrderOrReplication(out double time, out int id, out DamageInfo info);
                    GetOrCreatePlayer(id).PutDamage(time, info);
                    if (info.offenderId == m_localPlayer.Id)
                    {
                        m_localPlayer.PutHitConfirm(time, info.offenseType);
                    }
                }
                break;

                case EMessage.ShootReplication:
                {
                    _reader.ReadShotReplication(out double time, out int id, out ShotInfo info);
                    NetPlayer player = GetOrCreatePlayer(id) as NetPlayer;
                    player?.PutShot(time, info);
                }
                break;

                case EMessage.KillSync:
                {
                    _reader.ReadKillSync(out double time, out KillInfo info);
                    Player killer = GetOrCreatePlayer(info.killerId);
                    Player victim = GetOrCreatePlayer(info.victimId);
                    killer.KillsValue.Put(time, info.killerKills);
                    victim.DeathsValue.Put(time, info.victimDeaths);
                    EventBoardBehaviour.Instance.Put(m_time, new EventBoardBehaviour.KillEvent
                    {
                        killer = killer,
                        victim = victim,
                        offenseType = info.offenseType
                    });
                    UpdateScoreBoard();
                }
                break;
            }
        }
    }
}