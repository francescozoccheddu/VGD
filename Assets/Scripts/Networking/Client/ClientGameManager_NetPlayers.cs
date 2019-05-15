using System.Collections.Generic;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Offense;

namespace Wheeled.Networking.Client
{
    public sealed partial class ClientGameManager
    {
        private sealed class NetPlayer : ClientPlayer
        {
            public float AverageReplicationInterval => m_replicationTapper.AverageInterval;
            public override bool IsLocal => false;

            private readonly TimeConstants.Tapper m_replicationTapper;
            private readonly ClientGameManager m_manager;

            private bool m_wasAlive;

            public NetPlayer(ClientGameManager _manager, byte _id, OffenseBackstage _offenseBackstage) : base(_manager, _id, _offenseBackstage)
            {
                m_replicationTapper = new TimeConstants.Tapper(1.0f);
                m_wasAlive = false;
                m_manager = _manager;
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
                bool isAlive = LifeHistory.IsAlive(m_manager.m_time);
                if (!m_wasAlive && isAlive)
                {
                    m_replicationTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }
        }
    }
}