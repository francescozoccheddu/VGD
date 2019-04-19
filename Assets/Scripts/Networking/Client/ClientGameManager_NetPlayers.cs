using System.Collections.Generic;
using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking.Client
{
    internal sealed partial class ClientGameManager
    {
        #region Private Classes

        private sealed class NetPlayer : PlayerBase
        {
            #region Public Properties

            public float AverageReplicationInterval => m_replicationTapper.AverageInterval;
            public override bool IsLocal => false;

            #endregion Public Properties

            #region Private Fields

            private readonly TimeConstants.Tapper m_replicationTapper;
            private readonly ClientGameManager m_manager;

            private bool m_wasAlive;

            #endregion Private Fields

            #region Public Constructors

            public NetPlayer(ClientGameManager _manager, byte _id) : base(_manager, _id)
            {
                m_replicationTapper = new TimeConstants.Tapper(1.0f);
                m_wasAlive = false;
                m_manager = _manager;
            }

            #endregion Public Constructors

            #region Public Methods

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

            #endregion Public Methods

            #region Protected Methods

            protected override void OnUpdated()
            {
                bool isAlive = LifeHistory.IsAlive(m_manager.m_time);
                if (!m_wasAlive && isAlive)
                {
                    m_replicationTapper.QuietTap();
                }
                m_wasAlive = isAlive;
            }

            #endregion Protected Methods
        }

        #endregion Private Classes
    }
}