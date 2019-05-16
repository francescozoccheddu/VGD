using System.Collections.Generic;
using Wheeled.Core;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Offense;
using Wheeled.Gameplay.Player;

namespace Wheeled.Networking.Client
{
    public sealed partial class ClientGameManager
    {
        private sealed class NetPlayer : ClientPlayer
        {
            public float AverageReplicationInterval => m_replicationTapper.AverageInterval;

            private readonly TimeConstants.Tapper m_replicationTapper;

            private bool m_wasAlive;

            public NetPlayer(int _id, OffenseBackstage _offenseBackstage) : base(_id, _offenseBackstage, false)
            {
                m_replicationTapper = new TimeConstants.Tapper(1.0f);
                m_wasAlive = false;
            }

            public void SignalReplication()
            {
                m_replicationTapper.Tap();
            }

            public void Move(int _step, IEnumerable<InputStep> _reversedInputSteps, Snapshot _snapshot)
            {
                int step = _step;
                foreach (InputStep inputStep in _reversedInputSteps)
                {
                    PutInput(step, inputStep);
                    step--;
                }
                PutSimulation(_step, _snapshot.simulation);
                PutSight(_step, _snapshot.sight);
            }

            protected override void OnUpdated()
            {
                bool isAlive = LifeHistory.IsAlive(GameManager.Current.Time);
                if (!m_wasAlive && isAlive)
                {
                    m_replicationTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }
        }
    }
}