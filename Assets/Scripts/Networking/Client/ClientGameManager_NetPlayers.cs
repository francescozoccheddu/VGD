using System.Collections.Generic;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        private sealed class NetPlayer : Player
        {

            private readonly TimeConstants.Tapper m_replicationTapper;
            private readonly ClientGameManager m_manager;

            public NetPlayer(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_replicationTapper = new TimeConstants.Tapper(1.0f);
                m_wasAlive = false;
                m_manager = _manager;
            }

            private bool m_wasAlive;

            public float AverageReplicationInterval => m_replicationTapper.AverageInterval;

            public void SignalReplication()
            {
                m_replicationTapper.Tap();
            }

            protected override void OnUpdated()
            {
                bool isAlive = EventHistory.IsAlive(m_manager.m_time);
                if (!m_wasAlive && isAlive)
                {
                    m_replicationTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }

            public override bool IsLocal => false;

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
        }
    }
}