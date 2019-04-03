using System.Linq;
using UnityEngine;
using Wheeled.Core.Utils;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay.Action
{
    internal sealed class ActionValidator
    {
        private const float c_maxShotPositionTolerance = 0.5f;
        private readonly LinkedListHistory<double, INode> m_history;
        private double m_maxAnticipation;
        private double? m_time;

        public ActionValidator()
        {
            m_history = new LinkedListHistory<double, INode>();
            MaxAnticipation = 1.0;
        }

        public interface ITarget
        {
            void Kaze(double _time);

            void Shoot(double _time, ShotInfo _info);
        }

        private interface INode
        {
        }

        public double MaxAnticipation { get => m_maxAnticipation; set { Debug.Assert(value > 0.0); m_maxAnticipation = value; } }
        public ITarget Target { get; set; }

        public void PutKaze(double _time)
        {
            if (_time >= m_time && _time <= m_time + MaxAnticipation)
            {
                m_history.Add(_time, new KazeNode());
            }
        }

        public void PutShot(double _time, ShotInfo _info)
        {
            if (_time >= m_time && _time <= m_time + MaxAnticipation)
            {
                m_history.Add(_time, new ShotNode { info = _info });
            }
        }

        public void SetAt(double _time)
        {
            m_time = _time;
        }

        public void ValidateUntil(double _time, ActionHistory _actionHistory, MovementHistory _movementHistory, InputHistory _inputHistory, byte _id)
        {
            m_time = _time;
            foreach ((double time, INode node) in m_history.GetFullSequence().Where(_n => _n.time <= _time))
            {
                switch (node)
                {
                    case KazeNode kazeNode:
                    if (_actionHistory.CanKaze)
                    {
                        Target?.Kaze(time);
                    }
                    break;

                    case ShotNode shotNode:
                    {
                        _movementHistory.GetSimulation(time, out SimulationStep? simulation, _inputHistory);
                        if (simulation != null && Vector3.Distance(simulation.Value.position, shotNode.info.position) <= c_maxShotPositionTolerance)
                        {
                            if ((shotNode.info.isRocket && _actionHistory.CanShootRocket)
                                || (!shotNode.info.isRocket && _actionHistory.CanShootRifle))
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

        private struct KazeNode : INode { }

        private struct ShotNode : INode
        {
            public ShotInfo info;
        }
    }
}