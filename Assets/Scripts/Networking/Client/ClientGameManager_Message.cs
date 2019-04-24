using System.Collections.Generic;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;
using Wheeled.HUD;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        #region Private Fields

        private const int c_maxStepAdvance = 30;

        #endregion Private Fields

        #region Public Methods

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
                    m_targetTime = time + m_server.Ping;
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
                    GetOrCreatePlayer(id).Info = info;
                    UpdateScoreBoard();
                }
                break;

                case Message.RecapSync:
                {
                    _reader.ReadRecapSync(out double time, out IEnumerable<PlayerRecapInfo> infos);
                    Dictionary<byte, ClientPlayer> oldPlayers = new Dictionary<byte, ClientPlayer>(m_players);
                    foreach (PlayerRecapInfo info in infos)
                    {
                        Player player = GetOrCreatePlayer(info.id);
                        player.DeathsValue.Put(time, info.deaths);
                        player.PutHealth(time, info.health);
                        player.KillsValue.Put(time, info.kills);
                        oldPlayers.Remove(info.id);
                    }
                    foreach (NetPlayer oldPlayer in oldPlayers.Values)
                    {
                        oldPlayer.PutQuit(time);
                    }
                    UpdateScoreBoard();
                }
                break;

                case Message.QuitReplication:
                {
                    _reader.ReadQuitReplication(out double time, out byte id);
                    if (m_players.TryGetValue(id, out ClientPlayer player))
                    {
                        player.PutQuit(time);
                    }
                    UpdateScoreBoard();
                }
                break;

                #endregion Room messages

                #region Movement messages

                case Message.SimulationOrder:
                {
                    _reader.ReadSimulationOrder(out int step, out SimulationStepInfo simulation);
                    if (m_isRunning && step > m_time.CeilingSimulationSteps() + c_maxStepAdvance)
                    {
                        break;
                    }
                    m_localPlayer.Correct(step, simulation);
                }
                break;

                case Message.MovementReplication:
                {
                    _reader.ReadMovementReplication(out byte id, out int step, out IEnumerable<InputStep> inputSteps, out Snapshot snapshot);
                    if (m_isRunning && step > m_time.CeilingSimulationSteps() + c_maxStepAdvance)
                    {
                        break;
                    }
                    NetPlayer player = GetOrCreatePlayer(id) as NetPlayer;
                    player?.SignalReplication();
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

                case Message.DamageOrderOrReplication:
                {
                    _reader.ReadDamageOrderOrReplication(out double time, out byte id, out DamageInfo info);
                    GetOrCreatePlayer(id).PutDamage(time, info);
                    if (info.offenderId == m_localPlayer.Id)
                    {
                        m_localPlayer.PutHitConfirm(time, info.offenseType);
                    }
                }
                break;

                case Message.ShootReplication:
                {
                    _reader.ReadShotReplication(out double time, out byte id, out ShotInfo info);
                    NetPlayer player = GetOrCreatePlayer(id) as NetPlayer;
                    player?.PutShot(time, info);
                }
                break;

                case Message.KillSync:
                {
                    _reader.ReadKillSync(out double time, out KillInfo info);
                    Player killer = GetOrCreatePlayer(info.killerId);
                    Player victim = GetOrCreatePlayer(info.victimId);
                    killer.KillsValue.Put(time, info.killerKills);
                    victim.DeathsValue.Put(time, info.victimDeaths);
                    MatchBoard.Put(m_time, new MatchBoard.KillEvent
                    {
                        killer = killer,
                        victim = victim,
                        offenseType = info.offenseType
                    });
                    UpdateScoreBoard();
                }
                break;

                #endregion Action messages
            }
        }

        #endregion Public Methods
    }
}