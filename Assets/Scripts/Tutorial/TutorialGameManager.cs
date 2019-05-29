using System.Collections.Generic;
using System.Linq;
using Wheeled.Core;
using Wheeled.Core.Data;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.Player;
using Wheeled.Networking;
using Wheeled.Scene;

namespace Wheeled.Tutorial
{
    public sealed class TutorialGameManager : IGameManager, Updatable.ITarget, OffenseBackstage.IValidationTarget
    {


        public double Time { get; private set; }

        GameRoomInfo IGameManager.Room { get; } = new GameRoomInfo();

        IEnumerable<IReadOnlyPlayer> IGameManager.Players => Enumerable.Repeat(m_player, 1);

        IReadOnlyPlayer IGameManager.LocalPlayer => m_player;

        IReadOnlyPlayer IGameManager.GetPlayerById(int _id) => _id == m_player.Id ? m_player : null;


        private readonly TutorialPlayer m_player;
        private readonly OffenseBackstage m_offenseBackstage;

        public TutorialGameManager()
        {
            GameManager.SetCurrentGameManager(this);
            m_offenseBackstage = new OffenseBackstage
            {
                ValidationTarget = this
            };
            m_player = new TutorialPlayer(m_offenseBackstage);
            m_player.Info = PlayerPreferences.Info;
            new Updatable(this, false)
            {
                IsRunning = true
            };
            m_player.PutSpawn(0.0, new SpawnInfo()
            {
                spawnPoint = SpawnManagerBehaviour.Spawn(0.0)
            });
        }

        void Updatable.ITarget.Update()
        {
            Time += UnityEngine.Time.deltaTime;
            m_offenseBackstage.UpdateUntil(Time);
            m_player.Update();
        }

        IEnumerable<OffenseBackstage.HitTarget> OffenseBackstage.IValidationTarget.ProvideHitTarget(double _time, Offense _offense) => Enumerable.Empty<OffenseBackstage.HitTarget>();
        void OffenseBackstage.IValidationTarget.Damage(double _time, int _offendedId, Offense _offense, float _damage) { }
        bool OffenseBackstage.IValidationTarget.ShouldProcess(double _time, Offense _offense) => true;

    }
}
