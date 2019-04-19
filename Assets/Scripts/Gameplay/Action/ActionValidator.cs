using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Networking;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionValidator
    {
        #region Public Interfaces

        public interface ITarget
        {
            #region Public Methods

            void Kaze(double _time, KazeInfo _info);

            void Shoot(double _time, ShotInfo _info);

            #endregion Public Methods
        }

        #endregion Public Interfaces

        #region Private Interfaces

        private interface INode
        {
        }

        #endregion Private Interfaces

        #region Private Structs

        private struct KazeNode : INode
        {
            #region Internal Fields

            internal KazeInfo info;

            #endregion Internal Fields
        }

        private struct ShotNode : INode
        {
            #region Public Fields

            public ShotInfo info;

            #endregion Public Fields
        }

        #endregion Private Structs

        #region Public Properties

        public double MaxAnticipation { get => m_maxAnticipation; set { Debug.Assert(value > 0.0); m_maxAnticipation = value; } }
        public double MaxDelay { get => m_maxDelay; set { Debug.Assert(value >= 0.0); m_maxDelay = value; } }
        public ITarget Target { get; set; }

        #endregion Public Properties

        #region Private Fields

        private const float c_maxShotPositionTolerance = 0.5f;
        private const float c_maxKazePositionTolerance = 0.8f;
        private readonly LinkedListHistory<double, INode> m_history;
        private double m_maxAnticipation;
        private double m_maxDelay;
        private double? m_time;

        #endregion Private Fields

        #region Public Constructors

        public ActionValidator()
        {
            m_history = new LinkedListHistory<double, INode>();
            MaxAnticipation = 1.0;
        }

        #endregion Public Constructors

        #region Public Methods

        public void PutKaze(double _time, KazeInfo _info)
        {
            if (_time >= m_time - MaxDelay && _time <= m_time + MaxAnticipation)
            {
                m_history.Add(_time, new KazeNode { info = _info });
            }
        }

        public void PutShot(double _time, ShotInfo _info)
        {
            if (_time >= m_time - MaxDelay && _time <= m_time + MaxAnticipation)
            {
                m_history.Add(_time, new ShotNode { info = _info });
            }
        }

        public void SetAt(double _time)
        {
            m_time = _time;
        }

        public void ValidateUntil(double _time, IReadOnlyPlayer _player)
        {
            m_time = _time;
            foreach ((double time, INode node) in m_history.GetFullSequence().Where(_n => _n.time <= _time))
            {
                switch (node)
                {
                    case KazeNode kazeNode:
                    if (Vector3.Distance(_player.GetSnapshot(time).simulation.Position, kazeNode.info.position) <= c_maxShotPositionTolerance)
                    {
                        if (!_player.LifeHistory.IsExploded(_time))
                        {
                            Target?.Kaze(time, kazeNode.info);
                        }
                    }

                    break;

                    case ShotNode shotNode:
                    {
                        if (Vector3.Distance(_player.GetSnapshot(time).simulation.Position, shotNode.info.position) <= c_maxShotPositionTolerance)
                        {
                            if (_player.LifeHistory.IsAlive(_time)
                                && ((shotNode.info.isRocket && _player.WeaponsHistory.CanShootRocket(time))
                                    || (!shotNode.info.isRocket && _player.WeaponsHistory.CanShootRifle(time, out _))))
                            {
                                Target.Shoot(time, shotNode.info);
                            }
                        }
                    }
                    break;
                }
            }
            m_history.ForgetAndOlder(_time);
        }

        #endregion Public Methods
    }
}