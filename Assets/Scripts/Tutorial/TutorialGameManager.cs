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
    public sealed class TutorialGameManager : IGameManager, Updatable.ITarget
    {

        private sealed class TutorialPlayer : Player
        {

            private readonly OffenseBackstage m_offenseBackstage;

            public static TutorialPlayer Create() => new TutorialPlayer(new OffenseBackstage());

            public TutorialPlayer(OffenseBackstage _offenseBackstage) : base(0, _offenseBackstage, true) => m_offenseBackstage = _offenseBackstage;

            protected override void OnUpdated()
            {
                base.OnUpdated();
                m_offenseBackstage.UpdateUntil(LocalTime);
            }

        }

        public double Time { get; private set; }

        GameRoomInfo IGameManager.Room { get; } = new GameRoomInfo();

        IEnumerable<IReadOnlyPlayer> IGameManager.Players => Enumerable.Repeat(m_player, 1);

        IReadOnlyPlayer IGameManager.LocalPlayer => m_player;

        IReadOnlyPlayer IGameManager.GetPlayerById(int _id) => _id == m_player.Id ? m_player : null;


        private readonly TutorialPlayer m_player;

        public TutorialGameManager()
        {
            GameManager.SetCurrentGameManager(this);
            m_player = TutorialPlayer.Create();
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
            m_player.Update();
        }
    }
}
